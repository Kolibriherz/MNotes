-- =============================================================================
-- MNotes - Database Schema
-- Target: PostgreSQL 18
--
-- Usage:
--   createdb mnotes
--   psql -d mnotes -f db/schema.sql
--
-- This script is additive and idempotent: every object is created with
-- IF NOT EXISTS, so it can be executed repeatedly against an existing database
-- without destroying data. It does not, however, migrate an existing schema to
-- a newer version - it only fills in what is missing.
--
-- To rebuild the database from scratch, run db/reset.sql first.
-- =============================================================================

SET search_path TO public;


-- =============================================================================
-- Functions
-- =============================================================================

-- Deactivates every event belonging to a note that is being deleted.
--
-- The eventstream is append-only: events are historical facts and are never
-- physically removed. When the owning note disappears, its events are flagged
-- as inactive instead, so the audit trail stays intact while queries can filter
-- them out. This is why the events keep an "ownerid" that no longer resolves to
-- an existing note - that is intentional, not a dangling reference.
CREATE OR REPLACE FUNCTION deactivate_events_for_deleted_note() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
begin
    update eventstream
    set active = false
    where ownerid = old.id;

    return old;
end;
$$;

COMMENT ON FUNCTION deactivate_events_for_deleted_note() IS
    'AFTER DELETE trigger function on "note": soft-deactivates the note''s events.';


-- =============================================================================
-- Tables
-- =============================================================================

-- Hierarchical folder structure for notes.
--
-- The hierarchy is modelled as an adjacency list. The root folder is a real row
-- with a fixed, well-known id (the all-zero UUID) that is its own parent. This
-- avoids a nullable "parentid" and the NULL handling that would come with it,
-- at the cost of requiring the root row to exist before any other row can be
-- inserted (see the seed section at the end of this file).
--
-- Known limitation: an adjacency list cannot prevent cycles (A -> B -> A) at
-- the schema level. Preventing them would require a recursive CHECK, which
-- PostgreSQL does not support, or a trigger walking the ancestor chain on every
-- write. The application layer is responsible for rejecting such moves.
CREATE TABLE IF NOT EXISTS folder (
    id           uuid                        DEFAULT gen_random_uuid() NOT NULL,
    name         character varying(255)                                NOT NULL,
    parentid     uuid                        DEFAULT '00000000-0000-0000-0000-000000000000'::uuid NOT NULL,
    creationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    doeom        timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT folder_pkey PRIMARY KEY (id),

    -- Only the root folder may be its own parent. Without this, any row could
    -- declare itself a second root and silently detach a whole subtree from the
    -- real one.
    CONSTRAINT ck_folder_single_root CHECK (
        (id = parentid) = (id = '00000000-0000-0000-0000-000000000000'::uuid)
    )
);

COMMENT ON TABLE  folder          IS 'Folder tree (adjacency list). The all-zero UUID is the root and is its own parent.';
COMMENT ON COLUMN folder.parentid IS 'Parent folder. Equals the own id only for the root folder.';
COMMENT ON COLUMN folder.doeom    IS 'Date of entry or modification. Maintained by the application layer, not by the database. A BEFORE UPDATE trigger would guarantee consistency even for manual SQL, at the price of moving behaviour into the database; the application is the single writer here, so the trigger is not worth the hidden magic.';


-- A single note. The core aggregate of the domain.
CREATE TABLE IF NOT EXISTS note (
    id           uuid                        DEFAULT gen_random_uuid() NOT NULL,
    name         character varying(255)                                NOT NULL,
    content      text,
    description  text,
    creationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    folderid     uuid                        DEFAULT '00000000-0000-0000-0000-000000000000'::uuid NOT NULL,
    doeom        timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT note_pk PRIMARY KEY (id)
);

COMMENT ON TABLE  note          IS 'A note. Deleting a note cascades to its comments and tag assignments.';
COMMENT ON COLUMN note.folderid IS 'Owning folder. Defaults to the root folder.';
COMMENT ON COLUMN note.doeom    IS 'Date of entry or modification. Maintained by the application layer, not by the database.';


-- Free-form label that can be attached to any number of notes.
CREATE TABLE IF NOT EXISTS tag (
    id    uuid                        DEFAULT gen_random_uuid() NOT NULL,
    name  character varying(255)                                NOT NULL,
    doeom timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT tag_pkey        PRIMARY KEY (id),
    CONSTRAINT unique_tag_name UNIQUE      (name)
);

COMMENT ON TABLE  tag       IS 'Tag. Names are globally unique.';
COMMENT ON COLUMN tag.doeom IS 'Date of entry or modification. Maintained by the application layer, not by the database.';


-- Join table resolving the many-to-many relationship between note and tag.
CREATE TABLE IF NOT EXISTS notetagassignment (
    id     uuid                        DEFAULT gen_random_uuid() NOT NULL,
    tagid  uuid                                                  NOT NULL,
    noteid uuid                                                  NOT NULL,
    doeom  timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT notetagassignment_pkey PRIMARY KEY (id),
    CONSTRAINT unique_tag_note        UNIQUE      (tagid, noteid)
);

COMMENT ON TABLE  notetagassignment       IS 'Many-to-many join between note and tag. A (tagid, noteid) pair is unique.';
COMMENT ON COLUMN notetagassignment.doeom IS 'Date of entry or modification. Maintained by the application layer, not by the database.';


-- Comment attached to a note.
--
-- A comment cannot exist without its note: "noteid" is NOT NULL and the row is
-- removed when the note is deleted. Modelling it as nullable would allow orphan
-- comments that no query in the domain has any use for.
CREATE TABLE IF NOT EXISTS comment (
    id           uuid                        DEFAULT gen_random_uuid() NOT NULL,
    noteid       uuid                                                  NOT NULL,
    content      text,
    creationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    doeom        timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT pk_comment PRIMARY KEY (id)
);

COMMENT ON TABLE  comment       IS 'Comment on a note. Cannot exist without its note.';
COMMENT ON COLUMN comment.doeom IS 'Date of entry or modification. Maintained by the application layer, not by the database.';


-- Append-only event log.
--
-- Rows are never updated or deleted by the application; the only mutation is the
-- trigger flipping "active" to false when the owning note is deleted.
-- "payloadtype" is the discriminator used to deserialize "payload" back into a
-- concrete .NET type, and must be part of the WHERE clause on every read to
-- prevent deserializing a payload into the wrong type.
CREATE TABLE IF NOT EXISTS eventstream (
    id          uuid                        DEFAULT gen_random_uuid() NOT NULL,
    ownerid     uuid                                                  NOT NULL,
    channel     character varying(255)                                NOT NULL,
    payloadtype character varying(255)                                NOT NULL,
    payload     text                                                  NOT NULL,
    active      boolean                     DEFAULT true              NOT NULL,
    doeom       timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT eventstream_pkey PRIMARY KEY (id)
);

COMMENT ON TABLE  eventstream             IS 'Append-only event log. Events are never physically deleted.';
COMMENT ON COLUMN eventstream.id          IS 'Normally assigned by the application; the default is a safety net for manual inserts.';
COMMENT ON COLUMN eventstream.active      IS 'Set to false by trigger when the owning note is deleted.';
COMMENT ON COLUMN eventstream.ownerid     IS 'Id of the owning entity. Deliberately not a foreign key: events outlive their owner.';
COMMENT ON COLUMN eventstream.payloadtype IS 'Type discriminator for "payload". Must be filtered on when reading.';
COMMENT ON COLUMN eventstream.doeom       IS 'Date of entry or modification. Maintained by the application layer, not by the database.';


-- =============================================================================
-- Foreign keys
--
-- PostgreSQL has no "ADD CONSTRAINT IF NOT EXISTS", so each constraint is added
-- inside a guarded DO block. This keeps the whole script re-runnable.
-- =============================================================================

DO $$
BEGIN
-- Deleting a non-root folder recursively deletes its entire subtree.
-- Notes contained in any of those folders are deleted as well. Their comments
-- and tag assignments are removed through the corresponding cascade rules,
-- while their history events are marked as inactive by the note-delete trigger.
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_parent_gid') THEN
        ALTER TABLE folder
            ADD CONSTRAINT fk_parent_gid FOREIGN KEY (parentid)
                REFERENCES folder (id) ON DELETE CASCADE;
    END IF;

    -- Deleting a folder deletes its notes.
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_folder') THEN
        ALTER TABLE note
            ADD CONSTRAINT fk_folder FOREIGN KEY (folderid)
                REFERENCES folder (id) ON DELETE CASCADE;
    END IF;

    -- Deleting a note deletes its comments.
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_note') THEN
        ALTER TABLE comment
            ADD CONSTRAINT fk_note FOREIGN KEY (noteid)
                REFERENCES note (id) ON DELETE CASCADE;
    END IF;

    -- Deleting a note deletes its tag assignments (but not the tags themselves).
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_note_gid') THEN
        ALTER TABLE notetagassignment
            ADD CONSTRAINT fk_note_gid FOREIGN KEY (noteid)
                REFERENCES note (id) ON DELETE CASCADE;
    END IF;

    -- Deleting a tag deletes its assignments (but not the notes).
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_tag_id') THEN
        ALTER TABLE notetagassignment
            ADD CONSTRAINT fk_tag_id FOREIGN KEY (tagid)
                REFERENCES tag (id) ON DELETE CASCADE;
    END IF;
END
$$;


-- =============================================================================
-- Indexes
--
-- PostgreSQL creates indexes for PRIMARY KEY and UNIQUE constraints, but NOT for
-- foreign key columns. Without these, every ON DELETE CASCADE forces a full
-- sequential scan of the child table, and every "give me the children of X"
-- query does the same.
-- =============================================================================

CREATE INDEX IF NOT EXISTS idx_folder_parentid          ON folder            (parentid);
CREATE INDEX IF NOT EXISTS idx_note_folderid            ON note              (folderid);
CREATE INDEX IF NOT EXISTS idx_comment_noteid           ON comment           (noteid);
CREATE INDEX IF NOT EXISTS idx_notetagassignment_noteid ON notetagassignment (noteid);

-- The (tagid, noteid) side is already covered by the unique_tag_note constraint,
-- which PostgreSQL can also use for lookups on tagid alone (leftmost prefix).

CREATE INDEX IF NOT EXISTS idx_eventstream_ownerid      ON eventstream       (ownerid);

-- Matches the primary read pattern of the eventstream:
-- WHERE channel = ? AND payloadtype = ? AND active = true
CREATE INDEX IF NOT EXISTS idx_eventstream_channel_type ON eventstream       (channel, payloadtype)
    WHERE active;


-- =============================================================================
-- Triggers
--
-- CREATE OR REPLACE TRIGGER requires PostgreSQL 14+.
-- =============================================================================

CREATE OR REPLACE TRIGGER trg_deactivate_events_after_note_delete
    AFTER DELETE ON note
    FOR EACH ROW
    EXECUTE FUNCTION deactivate_events_for_deleted_note();


-- =============================================================================
-- Seed data
--
-- The root folder is not "data" in the usual sense - it is structural. Both
-- folder.parentid and note.folderid default to its id, so no other row can be
-- inserted before it exists. It therefore lives in the schema file rather than
-- in a separate seed script.
--
-- ON CONFLICT DO NOTHING keeps the insert idempotent.
-- =============================================================================

INSERT INTO folder (id, name, parentid, creationdate, doeom)
VALUES ('00000000-0000-0000-0000-000000000000', 'Root', '00000000-0000-0000-0000-000000000000',
        CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;
