CREATE TABLE IF NOT EXISTS "crawl_rules" (
  "id" INTEGER(11) NOT NULL,
  "name" TEXT(20) NOT NULL,
  "url" TEXT(250) NOT NULL,
  "partten" TEXT(140) NOT NULL,
  "max_page" integer(5) NOT NULL,
  "enable" integer(1) NOT NULL,
  PRIMARY KEY ("id")
);
CREATE TABLE IF NOT EXISTS "pref" (
  "account_file_dir" TEXT(120),
  "tan8_home_dir" TEXT(120)
);
CREATE TABLE IF NOT EXISTS "proxy_address" (
  "address" TEXT(40) NOT NULL,
  "addtime" TEXT(25) NOT NULL,
  PRIMARY KEY ("address")
);
CREATE TABLE IF NOT EXISTS "tan8_music" (
  "ypid" INTEGER(11) NOT NULL,
  "name" TEXT(100) NOT NULL,
  "star" integer(1),
  "yp_count" integer(2),
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