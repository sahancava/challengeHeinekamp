# How to Run
To clone the repository, you can use either of the following commands:
```
gh repo clone sahancava/challengeHeinekamp
```
or
```
git clone https://github.com/sahancava/challengeHeinekamp.git
```

Navigate to the application folder:
```
cd challengeHeinekamp
```
For the first time setup, execute the following commands:
```
cd ClientApp && npm install
cd ..
dotnet run
```

**Please note that the initial start may take longer than usual as it sets up the dependencies. Also, ensure that you have .NET 6.0 installed on your system.**

# How to Test
## Post Run
[x] Select a (or multiple) .PDF file(s) after clicking Choose Files button. <br>
[x] Click 'Shareable File' button in case you'd like them to provide a URL to share public. <br>
[x] You'll need to set number of hours to expire value if you clicked 'Shareable File' option. <br>
[x] Click 'Upload' button. <br>
[x] You'll be able to see the file(s) you uploaded.<br>
[x] If you'd like to see the uploaded files data on the database, you'll need to execute the commands below at the root of the application:
```
sqlite3 mydb.db
```
and
```
SELECT * FROM files;
```

## File Preview
[x] Clicking the 'Preview' link of the uploaded files will show you the first page of the file.<br>

## Downloading a File
[x] If a file is shareable, you'll see 'Download' link at the action section. Clicking 'Download' link both will make you download the file and will increase the downloadCount by one **only if the expire date of the file is ahead of the current time.**

# Application Architecture Description  
This application follows a modern and user-friendly architecture, using React and React Bootstrap to create a simple and intuitive file management system. The architecture focuses on being easy to use, reusable, and providing a smooth experience for users.
  
## Front-End Architecture
The front-end is built with React, using a component-based structure for modularity and reusability. Separate components like the file table, preview modal, and upload form make the application flexible and adaptable.
  
## Component-based Structure  
The application is divided into separate components, such as the file table, file preview modal, and file upload form. Each component has its own logic and presentation, which allows us to use them in different parts of the application.
      
## Efficient State Management
React's useState hook is employed for dynamic data management, ensuring a responsive and interactive user interface.

## Asynchronous Operations
Asynchronous tasks like file uploads and data retrieval are handled using JavaScript's fetch API and async/await syntax, enhancing the overall user experience.

## Integration of Third-Party Libraries
Widely used libraries like jQuery, Font Awesome, and react-file-viewer are integrated to improve functionality and streamline development.

## Back-End Architecture
The provided back-end code includes a SampleController class that handles HTTP requests for file management.

### Endpoints
#### GET /Sample
Retrieves a list of files from an SQLite database.
#### POST /Sample/UploadFile
Handles file uploads and stores relevant information in the database.
#### GET /Sample/DownloadFile
Allows users to download files based on file ID and GUID.
### Database
An SQLite database is used to store file-related information, including file details and metadata.
### Error Handling
The code includes error handling logic to handle various scenarios and returns appropriate error responses with relevant messages.

## Overall Architecture Benefits
The chosen architecture offers several advantages:
### Modularity and Reusability:
The component-based structure enables the creation of reusable UI elements, reducing code duplication and facilitating maintenance.
### User-Friendly Interface:
React's state management and event handling capabilities ensure smooth interactions, providing a seamless and intuitive interface.
### Scalability:
he modular architecture allows for easy expansion and addition of new features or components, enabling the application to grow and evolve over time.


# Design Decisions

### Separation of Front-End and Back-End
The app is divided into front-end and back-end parts to facilitate independent development and scalability.
### RESTful API
The app follows RESTful API principles to ensure clear and standardized communication between the client and server.
### Database Storage
Files and their relevant information are stored in an SQLite database, enabling efficient data management.
### Error Handling and Validation
The code includes error handling and validation to ensure proper app functioning and secure user interactions.
### File Storage
Uploaded files are saved with unique names in a designated directory for easy management and retrieval.
### Shareable Files
The app supports sharing files with others by assigning unique identifiers and expiration times to ensure secure access.
### MIME Types and File Extensions
Uploaded files are validated based on their MIME types and extensions to prevent uploading unsupported or malicious files.

# Things That Can Be Improved
## Error Logging
Integration of error logging to catch the possible errors. For example, under Logs folder there may be .log files in the schema of YYYY-MM-DD that contains try-catch errors.
## File Search
There may be a search bar on the UI connecting to an endpoint, providing name or GUID to search the files.
## Pagination and Sorting
Pagination for easy the load on the UI and sorting for such properties like uploadDateTime, downloadCount or originalFileName
## UI Schema
At this version of the application, it's being considered only a structure in the UI since it almost 100% focused on the functional perspectives for example uploading, downloading and API endpoint design. At the stable production version of the application, it can widely focus on the end-user design such as using CSS and Bootstrap widely to make it responsive to the mobile devices.