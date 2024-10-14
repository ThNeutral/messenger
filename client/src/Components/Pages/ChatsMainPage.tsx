import React, { useEffect, useState } from 'react';
// import { useNavigate } from 'react-router-dom'; 
import '../../assets/css/chatPage.css';
import { MenuIcon } from '../Icons/MenuIcon';
import { LoupeIcon } from '../Icons/LoupeIcon';
import { Chats } from '../../Interfaces/Chats';
import { Menu } from '../Pop up/Menu';
export const ChatsMainPage = () => {
  const [chats, setChats] = useState <Chats[]> ([]);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  
//   const navigate = useNavigate(); 

//   const handleLogout = () => {
//     localStorage.removeItem('authToken'); 
//     navigate('/signin');
//   };
const toggleMenu = () => {
  setIsMenuOpen((prev) => !prev);
}
const closeMenu = () => {
  setIsMenuOpen(!isMenuOpen); 
};
 

  return (
    <div className="chats-main-container">
      <div className='allChats'>
      <div className={`menu-open ${isMenuOpen ? 'show' : ''}`}>
          <Menu isMenuOpenRef={isMenuOpen} setIsMenuOpen={setIsMenuOpen}/>
        </div>
        
        {isMenuOpen && <div className="overlay" onClick={closeMenu}></div>}
     <div className="input-wrapper">

     <span className='menu-icon-span' onClick={toggleMenu}><MenuIcon /></span>
     <input type="text" placeholder="Search" />
     <span className='loupe-icon-span'><LoupeIcon/></span>
     </div>
     <div>
          {/* <h3>Your Chats</h3> */}
          {chats.map((chat) => (
            <div key={chat.ChatID}>{chat.ChatName}</div>
          ))}
        </div>
     </div>
      <div className='messages'>ChatsMainPage</div>
      {/* <button onClick={handleLogout}>Logout</button> */}
    </div>
  );
}
