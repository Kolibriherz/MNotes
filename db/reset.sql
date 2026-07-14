-- =============================================================================
-- MNotes - Database Reset
-- Target: PostgreSQL 18
--
-- Usage:
--   psql -d mnotes -f db/reset.sql
--   psql -d mnotes -f db/schema.sql
--
-- WARNING: THIS SCRIPT DESTROYS ALL DATA.
--
-- It drops every object created by db/schema.sql. It exists so that schema.sql
-- can stay purely additive: rebuilding from scratch is an explicit, separate
-- act, not a side effect of applying the schema. Intended for local development
-- and CI only.
-- =============================================================================

SET search_path TO public;

-- Dropped in reverse dependency order. CASCADE removes dependent foreign keys
-- and the trigger on "note" automatically.
DROP TABLE IF EXISTS notetagassignment CASCADE;
DROP TABLE IF EXISTS comment           CASCADE;
DROP TABLE IF EXISTS eventstream       CASCADE;
DROP TABLE IF EXISTS note              CASCADE;
DROP TABLE IF EXISTS tag               CASCADE;
DROP TABLE IF EXISTS folder            CASCADE;

DROP FUNCTION IF EXISTS deactivate_events_for_deleted_note();
