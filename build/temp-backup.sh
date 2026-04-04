set -e
echo 'staring packup environment: production'

mkdir -p /var/www/dndocs/backups/staging-production
rm -rf /var/www/dndocs/backups/staging-production/*

echo 'copy to staging - some ssd disk to have first fast copy'

sqlite3 "file:/var/www/dndocs/dndocs-docs-data-production/app.sqlite?mode=rw" ".backup '/var/www/dndocs/backups/staging-production/app.sqlite'"
sqlite3 "file:/var/www/dndocs/dndocs-docs-data-production/site.sqlite?mode=rw" ".backup '/var/www/dndocs/backups/staging-production/site.sqlite'"
sqlite3 "file:/var/www/dndocs/dndocs-docs-data-production/varsite.sqlite?mode=rw" ".backup '/var/www/dndocs/backups/staging-production/varsite.sqlite'"

echo 'completed staging sqlite .backup, starting rsync to hdd as slow copy'

# copy that 'staging' from ssd to some hdd (slow disk) 
rsync -av --progress /var/www/dndocs/backups/staging-production/ /mnt/usb1/backups/dndocs-docs-data-production-2026-04-04_16-57-17/
rm -rf /var/www/dndocs/backups/staging-production/*

sqlite3 "file:/var/www/dndocs/dndocs-data-production/app.sqlite?mode=rw" ".backup '/var/www/dndocs/backups/staging-production/app.sqlite'"
rsync -av --progress /var/www/dndocs/backups/staging-production/ /mnt/usb1/backups/dndocs-data-production-2026-04-04_16-57-17/

# not remove 'staging'

#rsync -av --progress --log-file=$logFile $src $dest/dndocs-data-bak-2026-04-04_16-57-17
#rsync -av --progress --log-file=$logFile $src $dest/dndocs-docs-data-bak-2026-04-04_16-57-17
