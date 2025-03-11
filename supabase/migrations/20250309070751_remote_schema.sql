

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;


CREATE EXTENSION IF NOT EXISTS "pg_net" WITH SCHEMA "extensions";






CREATE EXTENSION IF NOT EXISTS "pgsodium";






COMMENT ON SCHEMA "public" IS 'standard public schema';



CREATE SCHEMA IF NOT EXISTS "testing";


ALTER SCHEMA "testing" OWNER TO "postgres";


CREATE EXTENSION IF NOT EXISTS "pg_graphql" WITH SCHEMA "graphql";






CREATE EXTENSION IF NOT EXISTS "pg_stat_statements" WITH SCHEMA "extensions";






CREATE EXTENSION IF NOT EXISTS "pgcrypto" WITH SCHEMA "extensions";






CREATE EXTENSION IF NOT EXISTS "pgjwt" WITH SCHEMA "extensions";






CREATE EXTENSION IF NOT EXISTS "supabase_vault" WITH SCHEMA "vault";






CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA "extensions";






CREATE OR REPLACE FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") RETURNS integer
    LANGUAGE "sql"
    AS $$SELECT COUNT(*)
FROM "EVENTS"
WHERE "EventCategoryTagId" = TagId
AND "Status" IN ('active');$$;


-- ALTER FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."count_participants"("event_uuid" "uuid") RETURNS integer
    LANGUAGE "sql" SECURITY DEFINER
    AS $$SELECT COUNT(*)
FROM "REGISTRATION"
WHERE "EventId" = event_uuid
AND "Status" IN ('confirmed');$$;


-- ALTER FUNCTION "public"."count_participants"("event_uuid" "uuid") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") RETURNS "text"
    LANGUAGE "plpgsql" SECURITY DEFINER
    SET "search_path" TO 'public'
    AS $$
    BEGIN
      IF NOT is_claims_admin() THEN
          RETURN 'error: access denied';
      ELSE
        update auth.users set raw_app_meta_data =
          raw_app_meta_data - claim where id = uid;
        return 'OK';
      END IF;
    END;
$$;


-- ALTER FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") RETURNS "jsonb"
    LANGUAGE "plpgsql" SECURITY DEFINER
    SET "search_path" TO 'public'
    AS $$
    DECLARE retval jsonb;
    BEGIN
      IF NOT is_claims_admin() THEN
          RETURN '{"error":"access denied"}'::jsonb;
      ELSE
        select coalesce(raw_app_meta_data->claim, null) from auth.users into retval where id = uid::uuid;
        return retval;
      END IF;
    END;
$$;


-- ALTER FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."get_claims"("uid" "uuid") RETURNS "jsonb"
    LANGUAGE "plpgsql" SECURITY DEFINER
    SET "search_path" TO 'public'
    AS $$
    DECLARE retval jsonb;
    BEGIN
      IF NOT is_claims_admin() THEN
          RETURN '{"error":"access denied"}'::jsonb;
      ELSE
        select raw_app_meta_data from auth.users into retval where id = uid::uuid;
        return retval;
      END IF;
    END;
$$;


-- ALTER FUNCTION "public"."get_claims"("uid" "uuid") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."get_my_claim"("claim" "text") RETURNS "jsonb"
    LANGUAGE "sql" STABLE
    AS $$
  select
  	coalesce(nullif(current_setting('request.jwt.claims', true), '')::jsonb -> 'app_metadata' -> claim, null)
$$;


-- ALTER FUNCTION "public"."get_my_claim"("claim" "text") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."get_my_claims"() RETURNS "jsonb"
    LANGUAGE "sql" STABLE
    AS $$
  select
  	coalesce(nullif(current_setting('request.jwt.claims', true), '')::jsonb -> 'app_metadata', '{}'::jsonb)::jsonb
$$;


-- ALTER FUNCTION "public"."get_my_claims"() OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."is_claims_admin"() RETURNS boolean
    LANGUAGE "plpgsql"
    AS $$
  BEGIN
    IF session_user = 'authenticator' THEN
      --------------------------------------------
      -- To disallow any authenticated app users
      -- from editing claims, delete the following
      -- block of code and replace it with:
      -- RETURN FALSE;
      --------------------------------------------
      IF extract(epoch from now()) > coalesce((current_setting('request.jwt.claims', true)::jsonb)->>'exp', '0')::numeric THEN
        return false; -- jwt expired
      END IF;
      If current_setting('request.jwt.claims', true)::jsonb->>'role' = 'service_role' THEN
        RETURN true; -- service role users have admin rights
      END IF;
      IF coalesce((current_setting('request.jwt.claims', true)::jsonb)->'app_metadata'->'claims_admin', 'false')::bool THEN
        return true; -- user has claims_admin set to true
      ELSE
        return false; -- user does NOT have claims_admin set to true
      END IF;
      --------------------------------------------
      -- End of block
      --------------------------------------------
    ELSE -- not a user session, probably being called from a trigger or something
      return true;
    END IF;
  END;
$$;


-- ALTER FUNCTION "public"."is_claims_admin"() OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") RETURNS "text"
    LANGUAGE "plpgsql" SECURITY DEFINER
    SET "search_path" TO 'public'
    AS $$
    BEGIN
      IF NOT is_claims_admin() THEN
          RETURN 'error: access denied';
      ELSE
        update auth.users set raw_app_meta_data =
          raw_app_meta_data ||
            json_build_object(claim, value)::jsonb where id = uid;
        return 'OK';
      END IF;
    END;
$$;


-- ALTER FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") OWNER TO "supabase_admin";


CREATE OR REPLACE FUNCTION "public"."update_event_status"("event_uuid" "uuid") RETURNS "void"
    LANGUAGE "plpgsql" SECURITY DEFINER
    AS $$DECLARE
    current_participants INTEGER;
    max_participants INTEGER;
BEGIN
    -- Get the current number of participants
    SELECT count_participants(event_uuid) INTO current_participants;

    -- Get the maximum number of participants for the event
    SELECT "MaxParticipant" INTO max_participants
    FROM "EVENTS"
    WHERE "EventId" = event_uuid;

    -- Check if the current number of participants exceeds the maximum
    IF current_participants >= max_participants THEN
        -- Update the event status to 'closed'
        UPDATE "EVENTS"
        SET "Status" = 'closed'
        WHERE "EventId" = event_uuid;
    END IF;
END;$$;


-- ALTER FUNCTION "public"."update_event_status"("event_uuid" "uuid") OWNER TO "supabase_admin";

SET default_tablespace = '';

SET default_table_access_method = "heap";


CREATE TABLE IF NOT EXISTS "public"."EVENTS" (
    "EventId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "CreatorUserId" "uuid" NOT NULL,
    "EventTitle" "text" NOT NULL,
    "EventDescription" "text",
    "EventCategoryTagId" "uuid",
    "EventLocationTagId" "uuid",
    "MaxParticipant" integer NOT NULL,
    "Cost" integer,
    "EventDate" timestamp with time zone,
    "PostExpiryDate" timestamp with time zone,
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "Deleted" boolean DEFAULT false NOT NULL,
    "Status" "text" DEFAULT 'active'::"text" NOT NULL,
    "FirstComeFirstServed" boolean DEFAULT false NOT NULL
);


-- ALTER TABLE "public"."EVENTS" OWNER TO "supabase_admin";


COMMENT ON COLUMN "public"."EVENTS"."FirstComeFirstServed" IS 'first come first serve';



CREATE TABLE IF NOT EXISTS "public"."EVENT_CATEGORY_TAG" (
    "EventCategoryTagId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "EventCategoryTagEmoji" "text" NOT NULL,
    "EventCategoryTagName" "text" NOT NULL,
    "Deleted" boolean DEFAULT false NOT NULL
);


-- ALTER TABLE "public"."EVENT_CATEGORY_TAG" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."EVENT_QUESTION" (
    "EventQuestionId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "EventId" "uuid" NOT NULL,
    "Question" "text" NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL
);


-- ALTER TABLE "public"."EVENT_QUESTION" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."LOCATION_TAG" (
    "LocationTagId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "LocationTagName" "text" NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "Deleted" boolean DEFAULT false NOT NULL
);


-- ALTER TABLE "public"."LOCATION_TAG" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."NOTIFICATIONS" (
    "NotificationId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "ToUserId" "uuid" NOT NULL,
    "FromUserId" "uuid" NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "Read" boolean DEFAULT false NOT NULL,
    "Title" "text" NOT NULL,
    "Message" "text" NOT NULL,
    "Url" "text"
);


-- ALTER TABLE "public"."NOTIFICATIONS" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."REGISTRATION" (
    "RegistrationId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "UserId" "uuid" NOT NULL,
    "EventId" "uuid" NOT NULL,
    "Status" "text",
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "UpdatedAt" timestamp with time zone DEFAULT "now"() NOT NULL
);


-- ALTER TABLE "public"."REGISTRATION" OWNER TO "supabase_admin";


COMMENT ON COLUMN "public"."REGISTRATION"."UpdatedAt" IS 'Timestamp when updated';



CREATE TABLE IF NOT EXISTS "public"."REGISTRATION_ANSWER" (
    "RegistrationAnswerId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "RegistrationId" "uuid" NOT NULL,
    "EventQuestionId" "uuid" NOT NULL,
    "Answer" "text",
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL
);


-- ALTER TABLE "public"."REGISTRATION_ANSWER" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."USERS" (
    "UserId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "Email" "text" NOT NULL,
    "ProfileImageURL" "text" NOT NULL,
    "DisplayName" "text" NOT NULL,
    "UserBio" "text",
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "Deleted" boolean DEFAULT false NOT NULL,
    "Username" "text" NOT NULL
);


-- ALTER TABLE "public"."USERS" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."USER_FEEDBACK" (
    "FeedbackId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "SourceUserId" "uuid" NOT NULL,
    "DestinationUserId" "uuid" NOT NULL,
    "ReviewScore" smallint DEFAULT '0'::smallint NOT NULL,
    "ReviewComment" "text",
    "CreatedAt" timestamp with time zone DEFAULT "now"() NOT NULL,
    "Deleted" boolean DEFAULT false
);


-- ALTER TABLE "public"."USER_FEEDBACK" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."USER_FOLLOWED" (
    "FollowingId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "FollowerUserId" "uuid" NOT NULL,
    "FollowedUserId" "uuid" NOT NULL
);


-- ALTER TABLE "public"."USER_FOLLOWED" OWNER TO "supabase_admin";


CREATE TABLE IF NOT EXISTS "public"."USER_FOLLOWED_CATEGORY" (
    "UserFollowedTagId" "uuid" DEFAULT "gen_random_uuid"() NOT NULL,
    "UserId" "uuid" NOT NULL,
    "EventCategoryTagId" "uuid"
);


-- ALTER TABLE "public"."USER_FOLLOWED_CATEGORY" OWNER TO "supabase_admin";


ALTER TABLE ONLY "public"."EVENTS"
    ADD CONSTRAINT "EVENTS_pkey" PRIMARY KEY ("EventId");



ALTER TABLE ONLY "public"."EVENT_CATEGORY_TAG"
    ADD CONSTRAINT "EVENT_CATEGORY_TAG_pkey" PRIMARY KEY ("EventCategoryTagId");



ALTER TABLE ONLY "public"."EVENT_QUESTION"
    ADD CONSTRAINT "EVENT_QUESTION_pkey" PRIMARY KEY ("EventQuestionId");



ALTER TABLE ONLY "public"."LOCATION_TAG"
    ADD CONSTRAINT "LOCATION_TAG_pkey" PRIMARY KEY ("LocationTagId");



ALTER TABLE ONLY "public"."NOTIFICATIONS"
    ADD CONSTRAINT "NOTIFICATIONS_pkey" PRIMARY KEY ("NotificationId");



ALTER TABLE ONLY "public"."REGISTRATION_ANSWER"
    ADD CONSTRAINT "REGISTRATION_ANSWER_pkey" PRIMARY KEY ("RegistrationAnswerId");



ALTER TABLE ONLY "public"."REGISTRATION"
    ADD CONSTRAINT "REGISTRATION_pkey" PRIMARY KEY ("RegistrationId");



ALTER TABLE ONLY "public"."USERS"
    ADD CONSTRAINT "USERS_Email_key" UNIQUE ("Email");



ALTER TABLE ONLY "public"."USERS"
    ADD CONSTRAINT "USERS_Username_key" UNIQUE ("Username");



ALTER TABLE ONLY "public"."USERS"
    ADD CONSTRAINT "USERS_pkey" PRIMARY KEY ("UserId");



ALTER TABLE ONLY "public"."USER_FEEDBACK"
    ADD CONSTRAINT "USER_FEEDBACK_pkey" PRIMARY KEY ("FeedbackId");



ALTER TABLE ONLY "public"."USER_FOLLOWED_CATEGORY"
    ADD CONSTRAINT "USER_FOLLOWED_CATEGORY_pkey" PRIMARY KEY ("UserFollowedTagId");



ALTER TABLE ONLY "public"."USER_FOLLOWED"
    ADD CONSTRAINT "USER_FOLLOWED_pkey" PRIMARY KEY ("FollowingId");



ALTER TABLE ONLY "public"."EVENTS"
    ADD CONSTRAINT "EVENTS_CreatorUserId_fkey" FOREIGN KEY ("CreatorUserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."EVENTS"
    ADD CONSTRAINT "EVENTS_EventCategoryTagId_fkey" FOREIGN KEY ("EventCategoryTagId") REFERENCES "public"."EVENT_CATEGORY_TAG"("EventCategoryTagId");



ALTER TABLE ONLY "public"."EVENTS"
    ADD CONSTRAINT "EVENTS_EventLocationTagId_fkey" FOREIGN KEY ("EventLocationTagId") REFERENCES "public"."LOCATION_TAG"("LocationTagId");



ALTER TABLE ONLY "public"."EVENT_QUESTION"
    ADD CONSTRAINT "EVENT_QUESTION_EventId_fkey" FOREIGN KEY ("EventId") REFERENCES "public"."EVENTS"("EventId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."NOTIFICATIONS"
    ADD CONSTRAINT "NOTIFICATIONS_FromUserId_fkey" FOREIGN KEY ("FromUserId") REFERENCES "public"."USERS"("UserId") ON UPDATE CASCADE ON DELETE SET NULL;



ALTER TABLE ONLY "public"."NOTIFICATIONS"
    ADD CONSTRAINT "NOTIFICATIONS_ToUserId_fkey" FOREIGN KEY ("ToUserId") REFERENCES "public"."USERS"("UserId") ON UPDATE CASCADE ON DELETE CASCADE;



ALTER TABLE ONLY "public"."REGISTRATION_ANSWER"
    ADD CONSTRAINT "REGISTRATION_ANSWER_EventQuestionId_fkey" FOREIGN KEY ("EventQuestionId") REFERENCES "public"."EVENT_QUESTION"("EventQuestionId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."REGISTRATION_ANSWER"
    ADD CONSTRAINT "REGISTRATION_ANSWER_RegistrationId_fkey" FOREIGN KEY ("RegistrationId") REFERENCES "public"."REGISTRATION"("RegistrationId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."REGISTRATION"
    ADD CONSTRAINT "REGISTRATION_EventId_fkey" FOREIGN KEY ("EventId") REFERENCES "public"."EVENTS"("EventId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."REGISTRATION"
    ADD CONSTRAINT "REGISTRATION_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."USERS"
    ADD CONSTRAINT "USERS_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES "auth"."users"("id") ON UPDATE CASCADE ON DELETE CASCADE;



ALTER TABLE ONLY "public"."USER_FEEDBACK"
    ADD CONSTRAINT "USER_FEEDBACK_DestinationUserId_fkey" FOREIGN KEY ("DestinationUserId") REFERENCES "public"."USERS"("UserId");



ALTER TABLE ONLY "public"."USER_FEEDBACK"
    ADD CONSTRAINT "USER_FEEDBACK_SourceUserId_fkey" FOREIGN KEY ("SourceUserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."USER_FOLLOWED_CATEGORY"
    ADD CONSTRAINT "USER_FOLLOWED_CATEGORY_EventCategoryTagId_fkey" FOREIGN KEY ("EventCategoryTagId") REFERENCES "public"."EVENT_CATEGORY_TAG"("EventCategoryTagId") ON DELETE SET NULL;



ALTER TABLE ONLY "public"."USER_FOLLOWED_CATEGORY"
    ADD CONSTRAINT "USER_FOLLOWED_CATEGORY_UserId_fkey" FOREIGN KEY ("UserId") REFERENCES "public"."USERS"("UserId") ON DELETE SET NULL;



ALTER TABLE ONLY "public"."USER_FOLLOWED"
    ADD CONSTRAINT "USER_FOLLOWED_DestinationUserId_fkey" FOREIGN KEY ("FollowedUserId") REFERENCES "public"."USERS"("UserId");



ALTER TABLE ONLY "public"."USER_FOLLOWED"
    ADD CONSTRAINT "USER_FOLLOWED_FollowedUserId_fkey" FOREIGN KEY ("FollowedUserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."USER_FOLLOWED"
    ADD CONSTRAINT "USER_FOLLOWED_FollowerUserId_fkey" FOREIGN KEY ("FollowerUserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE ONLY "public"."USER_FOLLOWED"
    ADD CONSTRAINT "USER_FOLLOWED_SourceUserId_fkey" FOREIGN KEY ("FollowerUserId") REFERENCES "public"."USERS"("UserId") ON DELETE CASCADE;



ALTER TABLE "public"."EVENTS" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."EVENT_CATEGORY_TAG" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."EVENT_QUESTION" ENABLE ROW LEVEL SECURITY;


CREATE POLICY "Enable authenticated event owner to view data" ON "public"."REGISTRATION" FOR SELECT TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "REGISTRATION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"())))));



CREATE POLICY "Enable delete for authenticated users based on FollowerUserId" ON "public"."USER_FOLLOWED" FOR DELETE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "FollowerUserId"));



CREATE POLICY "Enable delete for authenticated users based on SourceUserId" ON "public"."USER_FEEDBACK" FOR DELETE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "SourceUserId"));



CREATE POLICY "Enable delete for authenticated users based on ToUserId" ON "public"."NOTIFICATIONS" FOR DELETE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "ToUserId"));



CREATE POLICY "Enable delete for authenticated users based on UserId" ON "public"."USER_FOLLOWED_CATEGORY" FOR DELETE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



CREATE POLICY "Enable insert for authenticated admin" ON "public"."EVENT_CATEGORY_TAG" FOR INSERT TO "authenticated" WITH CHECK (("public"."get_my_claim"('app_role'::"text") = '"admin"'::"jsonb"));



CREATE POLICY "Enable insert for authenticated admin" ON "public"."LOCATION_TAG" FOR INSERT TO "authenticated" WITH CHECK (("public"."get_my_claim"('app_role'::"text") = '"admin"'::"jsonb"));



CREATE POLICY "Enable insert for authenticated user based on CreatorUserId" ON "public"."EVENT_QUESTION" FOR INSERT TO "authenticated" WITH CHECK ((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "EVENT_QUESTION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"())))));



CREATE POLICY "Enable insert for authenticated user based on registrations UID" ON "public"."REGISTRATION_ANSWER" FOR INSERT TO "authenticated" WITH CHECK ((EXISTS ( SELECT 1
   FROM "public"."REGISTRATION" "r"
  WHERE (("r"."RegistrationId" = "REGISTRATION_ANSWER"."RegistrationId") AND ("r"."UserId" = "auth"."uid"())))));



CREATE POLICY "Enable insert for authenticated user who owns the event" ON "public"."REGISTRATION_ANSWER" FOR SELECT TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM ("public"."REGISTRATION" "r"
     JOIN "public"."EVENTS" "e" ON (("r"."EventId" = "e"."EventId")))
  WHERE (("r"."RegistrationId" = "REGISTRATION_ANSWER"."RegistrationId") AND ("e"."CreatorUserId" = "auth"."uid"())))));



CREATE POLICY "Enable insert for authenticated users based on CreatorUserId" ON "public"."EVENTS" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "CreatorUserId"));



CREATE POLICY "Enable insert for authenticated users based on FollowerUserId" ON "public"."USER_FOLLOWED" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "FollowerUserId"));



CREATE POLICY "Enable insert for authenticated users based on FromUserId" ON "public"."NOTIFICATIONS" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "FromUserId"));



CREATE POLICY "Enable insert for authenticated users based on SourceUserId" ON "public"."USER_FEEDBACK" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "SourceUserId"));



CREATE POLICY "Enable insert for authenticated users based on UserId" ON "public"."REGISTRATION" FOR INSERT TO "authenticated" WITH CHECK (((NOT (EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "REGISTRATION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"()))))) AND (( SELECT "EVENTS"."MaxParticipant"
   FROM "public"."EVENTS"
  WHERE ("EVENTS"."EventId" = "REGISTRATION"."EventId")) > "public"."count_participants"("EventId")) AND ("UserId" = "auth"."uid"())));



CREATE POLICY "Enable insert for authenticated users based on UserId" ON "public"."USERS" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



CREATE POLICY "Enable insert for authenticated users based on UserId" ON "public"."USER_FOLLOWED_CATEGORY" FOR INSERT TO "authenticated" WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



CREATE POLICY "Enable read access for all users" ON "public"."EVENT_CATEGORY_TAG" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable read access for authenticated users" ON "public"."EVENTS" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable read access for authenticated users" ON "public"."EVENT_QUESTION" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable read access for authenticated users" ON "public"."LOCATION_TAG" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable read access for authenticated users" ON "public"."USERS" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable read access for authenticated users" ON "public"."USER_FEEDBACK" FOR SELECT TO "authenticated" USING (true);



CREATE POLICY "Enable select for authenticacted users based on ToUserId" ON "public"."NOTIFICATIONS" FOR SELECT TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "ToUserId"));



CREATE POLICY "Enable select for authenticated user who registered" ON "public"."REGISTRATION_ANSWER" FOR SELECT TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM "public"."REGISTRATION" "r"
  WHERE (("r"."RegistrationId" = "REGISTRATION_ANSWER"."RegistrationId") AND ("r"."UserId" = "auth"."uid"())))));



CREATE POLICY "Enable select for authenticated users based on FromUserId" ON "public"."NOTIFICATIONS" FOR SELECT TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "FromUserId"));



CREATE POLICY "Enable update for authed event creator to edit registration" ON "public"."REGISTRATION" FOR UPDATE TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "REGISTRATION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"())))));



CREATE POLICY "Enable update for authenticated admin" ON "public"."EVENT_CATEGORY_TAG" FOR UPDATE TO "authenticated" USING (("public"."get_my_claim"('app_role'::"text") = '"admin"'::"jsonb"));



CREATE POLICY "Enable update for authenticated admin" ON "public"."LOCATION_TAG" FOR UPDATE TO "authenticated" USING (("public"."get_my_claim"('app_role'::"text") = '"admin"'::"jsonb"));



CREATE POLICY "Enable update for authenticated user based on CreatorUserId" ON "public"."EVENT_QUESTION" FOR UPDATE TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "EVENT_QUESTION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"()))))) WITH CHECK ((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE (("e"."EventId" = "EVENT_QUESTION"."EventId") AND ("e"."CreatorUserId" = "auth"."uid"())))));



CREATE POLICY "Enable update for authenticated user who registered" ON "public"."REGISTRATION_ANSWER" FOR UPDATE TO "authenticated" USING ((EXISTS ( SELECT 1
   FROM "public"."REGISTRATION" "r"
  WHERE (("r"."RegistrationId" = "REGISTRATION_ANSWER"."RegistrationId") AND ("r"."UserId" = "auth"."uid"())))));



CREATE POLICY "Enable update for authenticated users based on SourceUserId" ON "public"."USER_FEEDBACK" FOR UPDATE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "SourceUserId")) WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "SourceUserId"));



CREATE POLICY "Enable update for authenticated users based on ToUserId" ON "public"."NOTIFICATIONS" FOR UPDATE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "ToUserId")) WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "ToUserId"));



CREATE POLICY "Enable update for authenticated users based on UserId" ON "public"."USERS" FOR UPDATE TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "UserId")) WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



CREATE POLICY "Enable update for authenticated users to edit their own" ON "public"."REGISTRATION" FOR UPDATE TO "authenticated" USING (((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE ("e"."CreatorUserId" <> "auth"."uid"()))) AND (( SELECT "auth"."uid"() AS "uid") = "UserId"))) WITH CHECK (((EXISTS ( SELECT 1
   FROM "public"."EVENTS" "e"
  WHERE ("e"."CreatorUserId" <> "auth"."uid"()))) AND (( SELECT "auth"."uid"() AS "uid") = "UserId")));



CREATE POLICY "Enable update for users based on CreatorUserId" ON "public"."EVENTS" FOR UPDATE USING ((( SELECT "auth"."uid"() AS "uid") = "CreatorUserId")) WITH CHECK ((( SELECT "auth"."uid"() AS "uid") = "CreatorUserId"));



CREATE POLICY "Enable users to view their own data only" ON "public"."REGISTRATION" FOR SELECT TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



CREATE POLICY "Enable users to view their own data only" ON "public"."USER_FOLLOWED" FOR SELECT TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "FollowerUserId"));



CREATE POLICY "Enable users to view their own data only" ON "public"."USER_FOLLOWED_CATEGORY" FOR SELECT TO "authenticated" USING ((( SELECT "auth"."uid"() AS "uid") = "UserId"));



ALTER TABLE "public"."LOCATION_TAG" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."NOTIFICATIONS" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."REGISTRATION" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."REGISTRATION_ANSWER" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."USERS" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."USER_FEEDBACK" ENABLE ROW LEVEL SECURITY;


ALTER TABLE "public"."USER_FOLLOWED_CATEGORY" ENABLE ROW LEVEL SECURITY;




ALTER PUBLICATION "supabase_realtime" OWNER TO "postgres";






ALTER PUBLICATION "supabase_realtime" ADD TABLE ONLY "public"."NOTIFICATIONS";






GRANT USAGE ON SCHEMA "public" TO "postgres";
GRANT USAGE ON SCHEMA "public" TO "anon";
GRANT USAGE ON SCHEMA "public" TO "authenticated";
GRANT USAGE ON SCHEMA "public" TO "service_role";




















































































































































































GRANT ALL ON FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") TO "postgres";
GRANT ALL ON FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") TO "anon";
GRANT ALL ON FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") TO "authenticated";
GRANT ALL ON FUNCTION "public"."GetEventCountByTagId"("tagid" "uuid") TO "service_role";



GRANT ALL ON FUNCTION "public"."count_participants"("event_uuid" "uuid") TO "postgres";
GRANT ALL ON FUNCTION "public"."count_participants"("event_uuid" "uuid") TO "anon";
GRANT ALL ON FUNCTION "public"."count_participants"("event_uuid" "uuid") TO "authenticated";
GRANT ALL ON FUNCTION "public"."count_participants"("event_uuid" "uuid") TO "service_role";



GRANT ALL ON FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") TO "postgres";
GRANT ALL ON FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") TO "anon";
GRANT ALL ON FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") TO "authenticated";
GRANT ALL ON FUNCTION "public"."delete_claim"("uid" "uuid", "claim" "text") TO "service_role";



GRANT ALL ON FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") TO "postgres";
GRANT ALL ON FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") TO "anon";
GRANT ALL ON FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") TO "authenticated";
GRANT ALL ON FUNCTION "public"."get_claim"("uid" "uuid", "claim" "text") TO "service_role";



GRANT ALL ON FUNCTION "public"."get_claims"("uid" "uuid") TO "postgres";
GRANT ALL ON FUNCTION "public"."get_claims"("uid" "uuid") TO "anon";
GRANT ALL ON FUNCTION "public"."get_claims"("uid" "uuid") TO "authenticated";
GRANT ALL ON FUNCTION "public"."get_claims"("uid" "uuid") TO "service_role";



GRANT ALL ON FUNCTION "public"."get_my_claim"("claim" "text") TO "postgres";
GRANT ALL ON FUNCTION "public"."get_my_claim"("claim" "text") TO "anon";
GRANT ALL ON FUNCTION "public"."get_my_claim"("claim" "text") TO "authenticated";
GRANT ALL ON FUNCTION "public"."get_my_claim"("claim" "text") TO "service_role";



GRANT ALL ON FUNCTION "public"."get_my_claims"() TO "postgres";
GRANT ALL ON FUNCTION "public"."get_my_claims"() TO "anon";
GRANT ALL ON FUNCTION "public"."get_my_claims"() TO "authenticated";
GRANT ALL ON FUNCTION "public"."get_my_claims"() TO "service_role";



GRANT ALL ON FUNCTION "public"."is_claims_admin"() TO "postgres";
GRANT ALL ON FUNCTION "public"."is_claims_admin"() TO "anon";
GRANT ALL ON FUNCTION "public"."is_claims_admin"() TO "authenticated";
GRANT ALL ON FUNCTION "public"."is_claims_admin"() TO "service_role";



GRANT ALL ON FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") TO "postgres";
GRANT ALL ON FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") TO "anon";
GRANT ALL ON FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") TO "authenticated";
GRANT ALL ON FUNCTION "public"."set_claim"("uid" "uuid", "claim" "text", "value" "jsonb") TO "service_role";



GRANT ALL ON FUNCTION "public"."update_event_status"("event_uuid" "uuid") TO "postgres";
GRANT ALL ON FUNCTION "public"."update_event_status"("event_uuid" "uuid") TO "anon";
GRANT ALL ON FUNCTION "public"."update_event_status"("event_uuid" "uuid") TO "authenticated";
GRANT ALL ON FUNCTION "public"."update_event_status"("event_uuid" "uuid") TO "service_role";


















GRANT ALL ON TABLE "public"."EVENTS" TO "postgres";
GRANT ALL ON TABLE "public"."EVENTS" TO "anon";
GRANT ALL ON TABLE "public"."EVENTS" TO "authenticated";
GRANT ALL ON TABLE "public"."EVENTS" TO "service_role";



GRANT ALL ON TABLE "public"."EVENT_CATEGORY_TAG" TO "postgres";
GRANT ALL ON TABLE "public"."EVENT_CATEGORY_TAG" TO "anon";
GRANT ALL ON TABLE "public"."EVENT_CATEGORY_TAG" TO "authenticated";
GRANT ALL ON TABLE "public"."EVENT_CATEGORY_TAG" TO "service_role";



GRANT ALL ON TABLE "public"."EVENT_QUESTION" TO "postgres";
GRANT ALL ON TABLE "public"."EVENT_QUESTION" TO "anon";
GRANT ALL ON TABLE "public"."EVENT_QUESTION" TO "authenticated";
GRANT ALL ON TABLE "public"."EVENT_QUESTION" TO "service_role";



GRANT ALL ON TABLE "public"."LOCATION_TAG" TO "postgres";
GRANT ALL ON TABLE "public"."LOCATION_TAG" TO "anon";
GRANT ALL ON TABLE "public"."LOCATION_TAG" TO "authenticated";
GRANT ALL ON TABLE "public"."LOCATION_TAG" TO "service_role";



GRANT ALL ON TABLE "public"."NOTIFICATIONS" TO "postgres";
GRANT ALL ON TABLE "public"."NOTIFICATIONS" TO "anon";
GRANT ALL ON TABLE "public"."NOTIFICATIONS" TO "authenticated";
GRANT ALL ON TABLE "public"."NOTIFICATIONS" TO "service_role";



GRANT ALL ON TABLE "public"."REGISTRATION" TO "postgres";
GRANT ALL ON TABLE "public"."REGISTRATION" TO "anon";
GRANT ALL ON TABLE "public"."REGISTRATION" TO "authenticated";
GRANT ALL ON TABLE "public"."REGISTRATION" TO "service_role";



GRANT ALL ON TABLE "public"."REGISTRATION_ANSWER" TO "postgres";
GRANT ALL ON TABLE "public"."REGISTRATION_ANSWER" TO "anon";
GRANT ALL ON TABLE "public"."REGISTRATION_ANSWER" TO "authenticated";
GRANT ALL ON TABLE "public"."REGISTRATION_ANSWER" TO "service_role";



GRANT ALL ON TABLE "public"."USERS" TO "postgres";
GRANT ALL ON TABLE "public"."USERS" TO "anon";
GRANT ALL ON TABLE "public"."USERS" TO "authenticated";
GRANT ALL ON TABLE "public"."USERS" TO "service_role";



GRANT ALL ON TABLE "public"."USER_FEEDBACK" TO "postgres";
GRANT ALL ON TABLE "public"."USER_FEEDBACK" TO "anon";
GRANT ALL ON TABLE "public"."USER_FEEDBACK" TO "authenticated";
GRANT ALL ON TABLE "public"."USER_FEEDBACK" TO "service_role";



GRANT ALL ON TABLE "public"."USER_FOLLOWED" TO "postgres";
GRANT ALL ON TABLE "public"."USER_FOLLOWED" TO "anon";
GRANT ALL ON TABLE "public"."USER_FOLLOWED" TO "authenticated";
GRANT ALL ON TABLE "public"."USER_FOLLOWED" TO "service_role";



GRANT ALL ON TABLE "public"."USER_FOLLOWED_CATEGORY" TO "postgres";
GRANT ALL ON TABLE "public"."USER_FOLLOWED_CATEGORY" TO "anon";
GRANT ALL ON TABLE "public"."USER_FOLLOWED_CATEGORY" TO "authenticated";
GRANT ALL ON TABLE "public"."USER_FOLLOWED_CATEGORY" TO "service_role";



ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON SEQUENCES  TO "service_role";






ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON FUNCTIONS  TO "service_role";






ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "postgres";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "anon";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "authenticated";
ALTER DEFAULT PRIVILEGES FOR ROLE "postgres" IN SCHEMA "public" GRANT ALL ON TABLES  TO "service_role";






























RESET ALL;
