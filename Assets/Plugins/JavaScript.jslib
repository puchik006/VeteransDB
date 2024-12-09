mergeInto(LibraryManager.library, {
  V_AddPhoto: function () {
    // Poll until the Unity instance is ready
    function waitForUnityInstance(callback) {
      if (window.myGameInstance) {
        console.log("Unity instance found.");
        callback();
      } else {
        console.log("Waiting for Unity instance...");
        setTimeout(() => waitForUnityInstance(callback), 100); // Retry after 100ms
      }
    }

    waitForUnityInstance(() => {
      console.log("Initializing file selection...");

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
          console.log("File selected: " + file.name);

          // Create a FileReader to read the image file
          var reader = new FileReader();
          reader.onload = function (event) {
            var base64Image = event.target.result; // Base64 encoded image

            console.log("Image read successfully, sending to Unity...");

            // Send the base64 image string back to Unity using SendMessage
            try {
              myGameInstance.SendMessage('LocalSettings', 'OnImageSelected', base64Image);
              console.log("Image sent to Unity successfully.");
            } catch (error) {
              console.error("Error sending message to Unity instance:", error);
            }
          };

          // Read the file as a data URL (base64 encoded)
          reader.readAsDataURL(file);
        } else {
          console.log("No file selected.");
        }
      });
    });
  }
});
