PRAGMA journal_mode=WAL;
PRAGMA page_size=4096;

BEGIN TRANSACTION;

create table app_log
(
    id integer primary key autoincrement,
    [message] text,
    category_name text,
    log_level_id int,
    event_id int,
    event_name text,
    [date] text
);

create table http_log
(
id integer primary key autoincrement,
[start_date] text,
end_date text,
write_log_date text,
client_ip text,
client_port int,
method text,
uri_path text,
uri_query text,
response_status int,
bytes_send int,
bytes_received int,
time_taken_ms int,
host text,
user_agent text,
referer text
);

COMMIT;