CREATE TABLE IF NOT EXISTS "tan8_music" (
  "ypid" INTEGER(11) NOT NULL,
  "name" TEXT(100) NOT NULL,
  "star" integer(1),
  "yp_count" integer(2),
  "ver" integer(1),
  "hot" integer(1),
  "origin_data" TEXT(1200),
  PRIMARY KEY ("ypid")
);
CREATE TABLE IF NOT EXISTS "tan8_music_down_record" (
  "id" INTEGER(11) NOT NULL,
  "ypid" integer(11),
  "name" TEXT(100),
  "code" integer(2),
  "result" TEXT(30),
  "create_time" text(25),
  "is_auto" integer(1),
  PRIMARY KEY ("id")
);
CREATE TABLE IF NOT EXISTS "app_secret_keys" (
  "secret_id" text(120) NOT NULL,
  "secret_key" TEXT(120) NOT NULL,
  "priv_domain" TEXT(30),
  "priv_sub_domain" TEXT(20),
  "platform" TEXT(20) NOT NULL,
  PRIMARY KEY ("secret_key")
);
CREATE TABLE IF NOT EXISTS "tan8_music_down_task" (
  "ypid" integer(11) NOT NULL,
  PRIMARY KEY ("ypid")
);
CREATE TABLE IF NOT EXISTS "search_kw_freq" (
  "kw" TEXT(50) NOT NULL,
  "freq" integer(20),
  "last_time" text(25),
  PRIMARY KEY ("kw")
);
CREATE TABLE IF NOT EXISTS "search_no_result_kw" (
  "kw" TEXT(50) NOT NULL,
  "freq" integer(20),
  "create_time" TEXT(25),
  PRIMARY KEY ("kw")
);
CREATE TABLE IF NOT EXISTS "tan8_music_img" (
  "ypid" INTEGER(11) NOT NULL,
  "yp_name" TEXT(200),
  "img_url" TEXT(160),
  "api" TEXT(20),
  "api_channel" TEXT(20),
  "status" INTEGER(1),
  "create_time" TEXT(25),
  "update_time" text(25),
  PRIMARY KEY ("ypid")
);
CREATE TABLE IF NOT EXISTS "media_sync_config" (
  "id" INTEGER(11) NOT NULL ON CONFLICT IGNORE,
  "pc_path" TEXT(200) NOT NULL,
  "mobile_path" TEXT(200) NOT NULL,
  "enable" integer(1) NOT NULL DEFAULT 1,
  PRIMARY KEY ("id"),
  CONSTRAINT "uk_pc_path" UNIQUE ("pc_path" ASC) ON CONFLICT REPLACE,
  CONSTRAINT "uk_mobile_path" UNIQUE ("mobile_path" ASC) ON CONFLICT REPLACE
);
CREATE TABLE IF NOT EXISTS "contact" (
  "id" INTEGER(11) NOT NULL,
  "platform" TEXT(10) NOT NULL,
  "uid" text(20) NOT NULL,
  "name" TEXT(25) NOT NULL,
  "phone" TEXT(20),
  "avatar" blob,
  PRIMARY KEY ("id")
);
CREATE INDEX IF NOT EXISTS 'INDEX_TAN8_NAME' ON 'tan8_music' ('name' ASC);