PRAGMA journal_mode=WAL;
PRAGMA page_size=4096;

BEGIN TRANSACTION;

create table shared_site_item
(
id integer primary key autoincrement,
[path] text,
byte_data BLOB,
sha_256 text
);

create table public_html
(
id integer primary key autoincrement,
[path] text,
byte_data BLOB,
created_on text,
updated_on text
);

create table sitemap_project
(
id integer primary key,
public_html_id int,
project_id int
);

COMMIT;