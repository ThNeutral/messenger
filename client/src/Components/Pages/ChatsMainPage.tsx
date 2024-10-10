import React from 'react';
import { useNavigate } from 'react-router-dom'; 

export const ChatsMainPage = () => {
  const navigate = useNavigate(); 

  const handleLogout = () => {
    localStorage.removeItem('authToken'); 
    navigate('/signin');
  };

  return (
    <>
      <div>ChatsMainPage</div>
      <button onClick={handleLogout}>Logout</button>
    </>
  );
}
