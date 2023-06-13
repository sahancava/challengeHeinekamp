import React, { useEffect, useState } from 'react';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import jQuery from 'jquery';
import FileViewer from 'react-file-viewer';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faFilePdf,
  faFileExcel,
  faFileWord,
  faFileText,
  faImage,
  faFileImage,
} from '@fortawesome/free-solid-svg-icons';

export function Home() {
  const [files, setFiles] = useState([]);
  const [selectedFiles, setSelectedFiles] = useState([]);
  const [fileNames, setFileNames] = useState([]);

  const [show, setShow] = useState(false);
  const [filePreviews, setFilePreviews] = useState({});
  const [shareable, setShareableFile] = useState(false);
  const [numberOfHoursToExpire, setNumberOfHoursToExpire] = useState(1);

  const handleClose = (file) => {
    setFilePreviews((prevPreviews) => ({
      ...prevPreviews,
      [file.id]: false,
    }));
  };

  const randomNames = ['John', 'Jane', 'Jack', 'Jill', 'Joe'];

  useEffect(() => {
    populateFilesData();
  }, []);

  const renderFilesTable = (files) => {
    const renderFontAwesomeIcon = (icon) => {
      return <FontAwesomeIcon icon={icon} />;
    };

    const iconTypes = {
      faFilePdf: renderFontAwesomeIcon(faFilePdf),
      faFileExcel: renderFontAwesomeIcon(faFileExcel),
      faFileWord: renderFontAwesomeIcon(faFileWord),
      faFileText: renderFontAwesomeIcon(faFileText),
      faImage: renderFontAwesomeIcon(faImage),
      faFileImage: renderFontAwesomeIcon(faFileImage),
    };

    const handlePreview = (event, file) => {
      const target = event.target;
      const rect = target.getBoundingClientRect();

      setFilePreviews((prevPreviews) => {
        const updatedPreviews = Object.keys(prevPreviews).reduce((acc, fileId) => {
          if (fileId !== file.id) {
            acc[fileId] = false;
          }
          return acc;
        }, {});
        return {
          ...updatedPreviews,
          [file.id]: true,
        };
      });

      setTimeout(() => {
        jQuery('.pg-viewer-wrapper').css('overflow-y', 'unset');
      }, 10);
    };
    if (files.length > 0) {
    return (
      <table className="table table-striped" aria-labelledby="tableLabel">
        <thead>
          <tr>
            <th>id</th>
            <th>icon</th>
            <th>originalFileName</th>
            <th>uploadDateTime</th>
            <th>downloadCount</th>
            <th>uploadedBy</th>
            <th>extension</th>
            <th>action</th>
          </tr>
        </thead>
        <tbody>
          {files.map((file) => (
            <React.Fragment key={file.id}>
              <tr>
                <td>{file.id}</td>
                <td>{iconTypes[file.icon]}</td>
                <td>{file.originalFileName}</td>
                <td>{file.uploadDateTime}</td>
                <td>{file.downloadCount}</td>
                <td>{file.uploadedBy}</td>
                <td>{file.extension}</td>
                <td className="exclude-hover">
                  [
                    {file.guid !== '00000000-0000-0000-0000-000000000000'
                    ?
                    <>
                    &nbsp;<a
                      style={{ color: 'red', textDecoration: 'none' }}
                      href={`sample/DownloadFile?id=${file.id}&guid=${file.guid}`}
                      download
                    >Download</a>
                    <span> |</span>
                    &nbsp;
                    <span style={{ color: 'red' }} onClick={() => getSharingURL(file.id, file.guid)}>Share URL</span>
                    
                    <span> |</span>
                    &nbsp;
                    </>
                    :
                    <></>}
                  &nbsp;<span style={{ color: 'red' }} onClick={(e) => handlePreview(e, file)}>Preview</span>&nbsp;]
                </td>
              </tr>
              {filePreviews[file.id] && (
                <Modal size="lg" show={true} onHide={() => handleClose(file)}>
                  <Modal.Header closeButton>
                    <Modal.Title>First Page Preview of {file.originalFileName}</Modal.Title>
                  </Modal.Header>
                  <Modal.Body>
                    <FileViewer fileType={file.extension} filePath={`files/${file.name}`} />
                  </Modal.Body>
                  <Modal.Footer>
                    <Button variant="secondary" onClick={() => handleClose(file)}>
                      Close The Preview
                    </Button>
                  </Modal.Footer>
                </Modal>
              )}
            </React.Fragment>
          ))}
        </tbody>
      </table>
    );
    } else {
      return (
        <div>
          <h2>No files found</h2>
        </div>
      );
    }
  };

  const populateFilesData = async () => {
    const response = await fetch('sample').then((response) => response.json());
    console.log(response);
    setFiles(response);
  };

  const handleFileSelect = (e) => {
    const selectedFiles = e.target.files;
    const fileNames = Array.from(selectedFiles).map((file) => file.name);

    setSelectedFiles(selectedFiles);
    setFileNames(fileNames);
  };

  const handleFileUpload = async () => {
    if (selectedFiles.length > 0) {
      if (shareable && numberOfHoursToExpire < 1) {
        alert("It cannot be below 1 once you select the Shareable File option.");
        return;
      }
      setShow(true);
      const uploadPromises = Array.from(selectedFiles).map(async (file) => {
        const formData = new FormData();
        formData.append('name', file['name']);
        formData.append('file', file);
        formData.append('uploadedBy', randomNames[Math.floor(Math.random() * randomNames.length)]);
        formData.append('shareableFile', shareable);
        formData.append('numberOfHoursToExpire', numberOfHoursToExpire);

        try {
          const response = await fetch('sample/UploadFile', {
            method: 'POST',
            body: formData,
          });
          const data = await response.json();
          if (data.error) {
            console.error(data.error);
            alert(data.error);
            jQuery('#beingUploaded').text(data.error);
          }
        } catch (error) {
          console.error(error);
        }
      });

      Promise.all(uploadPromises)
        .then(() => {
          setTimeout(() => {
            setShow(false);
            populateFilesData();
          }, 3000);
        })
        .catch((error) => {
          console.error(error);
          setShow(false);
        });
      
    }
  };

  const getSharingURL = (id, guid) => {
    alert(`${window.location.origin}/sample/DownloadFile?id=${id}&guid=${guid}`);
  }

  let contents = renderFilesTable(files);

  return (
    <div>
      <h1 id="tableLabel">Current Files</h1>
      {contents}

      <div className="upload-section">
        <h2>Upload File(s)</h2>
          <div className="form-group">
            <label htmlFor="file">File:</label>
            <input
              type="hidden"
              onChange={(e) => setFileNames(Array.from(e.target.value))}
              placeholder="Enter file names"
            />
            <input type="file" multiple onChange={handleFileSelect} name="file" id="file" />
          </div>
          <div className="form-group">
            <label htmlFor="shareableFile">Shareable File:</label>
            <input type="checkbox" onChange={(e) => setShareableFile(e.target.checked)} checked={shareable} name="shareableFile" id="shareableFile" />
          </div>
          <div className="form-group">
            <label htmlFor="numberOfHoursToExpire">Number of Hours to Expire:</label>
            <input min="1" disabled={!shareable} onChange={(e) => setNumberOfHoursToExpire(parseInt(e.target.value))} value={numberOfHoursToExpire} type="number" name="numberOfHoursToExpire" id="numberOfHoursToExpire" />
          </div>
          <div className="form-group">
            <button onClick={handleFileUpload} type="submit">Upload</button>
          </div>
      </div>

      <Modal show={show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Upload Process</Modal.Title>
        </Modal.Header>
        <Modal.Body id="beingUploaded">Your files are being uploaded...</Modal.Body>
      </Modal>
    </div>
  );
}
