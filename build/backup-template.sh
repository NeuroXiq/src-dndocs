if systemctl stop $dndocsServiceName && systemctl stop $dndocsDocsServiceName; then
    echo 'stopped ok'
else
    echo 'stopped failed'
    systemctl start $dndocsServiceName
    systemctl start $dndocsDocsServiceName
    exit 1
fi

rsync -av --progress $dndocsDocsDataPath/ /mnt/usb1/backups/dndocs-data-$environment-$bakDate/
rsync -av --progress $dndocsDataPath/ /mnt/usb1/backups/dndocs-data-$environment-$bakDate/

systemctl start $dndocsServiceName;
systemctl start $dndocsDocsServiceName;