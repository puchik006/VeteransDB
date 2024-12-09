mergeInto(LibraryManager.library, {
  V_AddPhoto: function () {
    // Create an HTML input element for file selection
    var fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.accept = "image/*"; // Restrict to image files

    // Trigger the file selection dialog
    fileInput.click();

    // When the user selects a file
    fileInput.addEventListener("change", function (e) {
      var file = e.target.files[0]; // Get the selected file
      if (file) {
        // Create a FileReader to read the image file
        var reader = new FileReader();
        reader.onload = function (event) {
          var base64Image = event.target.result; // Base64 encoded image

          // Send the base64 image string back to Unity using SendMessage
          if (myGameInstance) {
            myGameInstance.SendMessage('MyGameObject', 'OnImageSelected', base64Image);
          }
        };

        // Read the file as a data URL (base64 encoded)
        reader.readAsDataURL(file);
      }
    });
  }
});