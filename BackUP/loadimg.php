<?php
$upload_dir = 'img/';  // Directory where the image will be stored

// Enable error reporting for debugging
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Check if the file was uploaded
if (isset($_FILES['file'])) {
    $file = $_FILES['file'];
    $fileName = $_POST['fileName']; // Get the file name from the POST data
    $fileTmpName = $file['tmp_name'];
    $fileError = $file['error'];

    // Check for errors during upload
    if ($fileError === 0) {
        // Set the path to save the file
        $fileDestination = $upload_dir . $fileName . ".png"; 

        // Move the uploaded file to the destination directory
        if (move_uploaded_file($fileTmpName, $fileDestination)) {
            echo "File uploaded successfully!";
        } else {
            echo "Error moving the file to the destination folder.";
        }
    } else {
        echo "Error uploading file: " . $fileError;
    }
} else {
    echo "No file uploaded.";
}
?>