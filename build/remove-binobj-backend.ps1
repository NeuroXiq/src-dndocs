$objbin = gci -path '../backend' -filter 'bin' -recurse -attributes Directory
$objbin | remove-item -recurse -force
$objbin = gci -path '../backend' -filter 'obj' -recurse -attributes Directory
$objbin | remove-item -recurse -force
