<?php
header('Content-Type: application/json; charset=utf-8');

// Get the JSON data from POST request
$JS = $_POST["JS"];

// Decode the JSON string into an array
$arrJS = json_decode($JS, true);

// Convert the array back to JSON format, with pretty printing
$str = json_encode($arrJS, JSON_UNESCAPED_UNICODE | JSON_PRETTY_PRINT);

// SFTP server details
$host = '45.86.183.61';
$port = 22;
$username = 'root';
$password = '1234500000';  

// Establish SSH connection
$connection = ssh2_connect($host, $port);
if (ssh2_auth_password($connection, $username, $password)) {
    echo "Authentication successful!\n";

    // Open the SFTP stream
    $sftp = ssh2_sftp($connection);

    // Path to save the file on the remote server
    $remote_file = "ssh2.sftp://$sftp/var/www/html/ww2/data.json";

    // Save the JSON data to the file
    if (file_put_contents($remote_file, $str)) {
        echo "OK";
    } else {
        echo "ERROR SAVE DATA";
    }
} else {
    echo "Authentication failed!";
}
?>
