import React from 'react';
// import { useNavigate } from 'react-router-dom'; 
import '../../assets/css/chatPage.css';
import loupeIcon from '../../assets/loupe.svg';
import { MenuIcon } from '../Icons/MenuIcon';
import { LoupeIcon } from '../Icons/LoupeIcon';
export const ChatsMainPage = () => {
//   const navigate = useNavigate(); 

//   const handleLogout = () => {
//     localStorage.removeItem('authToken'); 
//     navigate('/signin');
//   };

  return (
    <div className="chats-main-container">
      <div className='allChats'>
    
     <div className="input-wrapper">
     <span className='menu-icon-span'><MenuIcon /></span>
     <input type="text" placeholder="Search" />
     <span className='loupe-icon-span'><LoupeIcon/></span>
     </div>
     
     </div>
      <div className='messages'>ChatsMainPage</div>
      {/* <button onClick={handleLogout}>Logout</button> */}
    </div>
  );
}
