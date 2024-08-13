PRAGMA journal_mode=WAL;
PRAGMA page_size=4096;

BEGIN TRANSACTION;

create table site_item
(
id integer primary key autoincrement,
project_id integer,
[path] text,
shared_site_item_id int,
byte_data blob
);

create index ix_site_item_project_id on site_item(project_id);

COMMIT;