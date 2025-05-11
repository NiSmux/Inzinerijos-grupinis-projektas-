import React, { useState } from 'react';
import api from './api'; // Import the API utility file
import './Settings.css'; // Import the styles

function Settings() {
  const [emailForm, setEmailForm] = useState({
    newEmail: '',
    currentPassword: '',
  });
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  });
  const [message, setMessage] = useState(''); // State for success/error messages
  const [errorDetails, setErrorDetails] = useState(null); // State for detailed errors from the backend

  // Function to handle changes in form fields (generic for both forms)
  const handleFormChange = (e, formType) => {
    const { name, value } = e.target;
    if (formType === 'email') {
      setEmailForm({ ...emailForm, [name]: value });
    } else { // formType === 'password'
      setPasswordForm({ ...passwordForm, [name]: value });
    }
  };

  const handleSubmitEmail = async (e) => {
    e.preventDefault(); // Prevent default form submission
    setMessage(''); // Clear previous messages
    setErrorDetails(null); // Clear previous errors

    console.log('Attempting to change email:', emailForm); // For debugging

    try {
      // Endpoint path with '/api' prefix (corrected in previous step)
      const response = await api.post('/api/Auth/change-email', emailForm);

      setMessage(response.data?.message || 'Email changed successfully.');
      setEmailForm({ newEmail: '', currentPassword: '' }); // Clear the form after success
    } catch (error) {
      console.error('Email change failed:', error.response?.data || error.message); // Log error details

      setMessage('Failed to change email.');
      if (error.response?.data?.message) {
           setMessage(`Failed to change email: ${error.response.data.message}`);
      } else if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
          const errors = error.response.data.errors.map(err => err.description || err);
          setErrorDetails(errors);
           setMessage('Failed to change email. Please check the errors below.');
      } else if (error.response?.data) {
           setErrorDetails([JSON.stringify(error.response.data)]); // Show error response as a string
           setMessage('Failed to change email. Details below.');
      } else {
         setMessage(`Failed to change email: ${error.message}`);
      }
    }
  };

  // Function to send the password change request
  const handleSubmitPassword = async (e) => {
    e.preventDefault(); // Prevent default form submission
    setMessage(''); // Clear previous messages
    setErrorDetails(null); // Clear previous errors

    // Frontend check for new password and confirmation match
    if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
      setMessage('New password and confirmation do not match');
      setErrorDetails(null);
      return;
    }

    console.log('Attempting to change password:', passwordForm); // For debugging

    try {
      // <-- CORRECTED LINE HERE -->
      // We now send the entire passwordForm state object as dataToSend.
      // This includes currentPassword, newPassword, AND confirmNewPassword.
      // The backend's [Compare] attribute needs confirmNewPassword to be present in the payload for validation.
      const dataToSend = passwordForm; // <-- CORRECTED: Send the whole form state


      // Endpoint path with '/api' prefix (corrected in previous step)
      const response = await api.post('/api/Auth/change-password', dataToSend); // This line remains the same

      setMessage(response.data?.message || 'Password changed successfully.');
      // Clear the form after success
      setPasswordForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    } catch (error) {
      console.error('Password change failed:', error.response?.data || error.message); // Log error details

      setMessage('Failed to change password.');
      if (error.response?.data?.message) {
           setMessage(`Failed to change password: ${error.response.data.message}`);
      } else if (error.response?.data?.errors && Array.isArray(error.response.data.errors)) {
          const errors = error.response.data.errors.map(err => err.description || err);
          setErrorDetails(errors);
           setMessage('Failed to change password. Please check the errors below.');
       } else if (error.response?.data) {
           setErrorDetails([JSON.stringify(error.response.data)]); // Show error response as a string
           setMessage('Failed to change password. Details below.');
       } else {
         setMessage(`Failed to change password: ${error.message}`);
       }
    }
  };

  return (
    <div className="settings-page-container">
      <div className="settings-content-wrapper"> {/* Container to center content */}
        <h2 className="settings-main-title">User Settings</h2>

        {/* Overall message */}
        {message && (
          <p className={`settings-message ${message.includes('successfully') ? 'success' : 'error'}`}> {/* <-- Check for "successfully" */}
            {message}
          </p>
        )}


        {errorDetails && errorDetails.length > 0 && (
          <div className="settings-error-details">
            <p>Details:</p>
            <ul>
              {errorDetails.map((detail, index) => (
                <li key={index}>{detail}</li>
              ))}
            </ul>
          </div>
        )}

        {/* Change Email Form */}
        <div className="settings-section glassmorphic-card">
          <h3>Change Email</h3>
          <form onSubmit={handleSubmitEmail}>
            <div className="form-group">
              <label htmlFor="newEmail">New Email:</label>
              <input
                type="email"
                id="newEmail"
                name="newEmail"
                value={emailForm.newEmail}
                onChange={(e) => handleFormChange(e, 'email')}
                required
                className="settings-input"
              />
            </div>
            <div className="form-group">
              <label htmlFor="currentPasswordEmail">Current Password:</label> {/* <-- Translated */}
              <input
                type="password"
                id="currentPasswordEmail"
                name="currentPassword"
                value={emailForm.currentPassword}
                onChange={(e) => handleFormChange(e, 'email')}
                required
                className="settings-input"
              />
            </div>
            <button type="submit" className="settings-button">Change Email</button> {/* <-- Translated */}
          </form>
        </div>

        {/* Change Password Form */}
        <div className="settings-section glassmorphic-card">
          <h3>Change Password</h3> {/* <-- Translated */}
          <form onSubmit={handleSubmitPassword}>
            <div className="form-group">
              <label htmlFor="currentPasswordPassword">Current Password:</label> {/* <-- Translated */}
              <input
                type="password"
                id="currentPasswordPassword"
                name="currentPassword"
                value={passwordForm.currentPassword}
                 onChange={(e) => handleFormChange(e, 'password')}
                required
                className="settings-input"
              />
            </div>
            <div className="form-group">
              <label htmlFor="newPassword">New Password:</label> {/* <-- Translated */}
              <input
                type="password"
                id="newPassword"
                name="newPassword"
                value={passwordForm.newPassword}
                 onChange={(e) => handleFormChange(e, 'password')}
                required
                className="settings-input"
              />
            </div>
            <div className="form-group">
              <label htmlFor="confirmNewPassword">Confirm New Password:</label> {/* <-- Translated */}
              <input
                type="password"
                id="confirmNewPassword"
                name="confirmNewPassword"
                value={passwordForm.confirmNewPassword}
                 onChange={(e) => handleFormChange(e, 'password')}
                required
                className="settings-input"
              />
            </div>
            <button type="submit" className="settings-button">Change Password</button> {/* <-- Translated */}
          </form>
        </div>
      </div>
    </div>
  );
}

export default Settings;
