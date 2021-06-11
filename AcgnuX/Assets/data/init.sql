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
INSERT INTO "crawl_rules" VALUES (1, '西刺国内高匿代理', 'https://www.xicidaili.com/nn/{0}', '<td>(\d+\.\d+\.\d+\.\d+)</td>\s+<td>(\d+)</td>', 2, 1);
INSERT INTO "crawl_rules" VALUES (2, '西刺国内普匿代理', 'https://www.xicidaili.com/nt/{0}', '<td>(\d+\.\d+\.\d+\.\d+)</td>\s+<td>(\d+)</td>', 1, 1);
INSERT INTO "crawl_rules" VALUES (3, '云代理', 'http://www.ip3366.net/free/?stype=1&page={0}', '<td>(\d+\.\d+\.\d+\.\d+)</td>\s+<td>(\d+)</td>', 4, 1);
INSERT INTO "crawl_rules" VALUES (4, 'IP海国内高匿代理', 'http://www.iphai.com/free/ng?p={0}', '<td>\s+(\d+\.\d+\.\d+\.\d+)\s+</td>\s+<td>\s+(\d+)\s+</td>', 1, 1);
INSERT INTO "crawl_rules" VALUES (5, 'IP海国外高匿代理', 'http://www.iphai.com/free/wg?p={0}', '<td>\s+(\d+\.\d+\.\d+\.\d+)\s+</td>\s+<td>\s+(\d+)\s+</td>', 1, 1);
INSERT INTO "crawl_rules" VALUES (6, '安小陌代理', 'http://www.66ip.cn/nmtq.php?getnum=10&isp=0&anonymoustype=3&start=&ports=&export=&ipaddress=&area=1&proxytype=2&api=66ip&p={0}', '(\d+\.\d+\.\d+\.\d+):(\d+)', 10, 1);
INSERT INTO "crawl_rules" VALUES (7, '快代理国内高匿', 'https://www.kuaidaili.com/free/inha/{0}/', '<td data-title="IP">\s+(\d+\.\d+\.\d+\.\d+)\s+</td>\s+<td data-title="PORT">\s+(\d+)\s+</td>', 2, 1);
INSERT INTO "crawl_rules" VALUES (8, '快代理国内普匿', 'https://www.kuaidaili.com/free/intr/{0}/', '<td data-title="IP">\s+(\d+\.\d+\.\d+\.\d+)\s+</td>\s+<td data-title="PORT">\s+(\d+)\s+</td>', 2, 1);
INSERT INTO "crawl_rules" VALUES (9, '89免费代理', 'http://www.89ip.cn/index_{0}.html', '<td>\s+(\d+\.\d+\.\d+\.\d+)\s+</td>\s+<td>\s+(\d+)\s+</td>', 60, 1);