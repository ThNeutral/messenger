import React, { useEffect, useState } from 'react';
// import { useNavigate } from 'react-router-dom'; 
import '../../assets/css/chatPage.css';
import { MenuIcon } from '../Icons/MenuIcon';
import { LoupeIcon } from '../Icons/LoupeIcon';
import { Chats } from '../../Interfaces/Chats';
import { Menu } from '../Pop up/Menu';
import { ChatInfo } from '../ChatInfo';
export const ChatsMainPage = () => {
  const [chats, setChats] = useState <Chats[]> ([]);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  
//   const navigate = useNavigate(); 

//   const handleLogout = () => {
//     localStorage.removeItem('authToken'); 
//     navigate('/signin');
//   };
useEffect(() => {
  const fetchChats = async () => {
      const token = localStorage.getItem('authToken');
      const response = await fetch('http://localhost:3000/get-my-chats', {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          }
      });
  
      if (response.ok) {
          const chats = await response.json();
          setChats(chats.chats);
          // console.log(chats);
      } else if (response.status === 404) {
          console.error('Chats not found: Redirecting to login');
          window.location.href = '/signin';
      } else if (response.status === 401) {
          console.error('Unauthorized: Redirecting to login');
          window.location.href = '/signin';
      }
  };

  fetchChats();
}, []);
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
     <div className='chats-container'>
          {/* <h3>Your Chats</h3> */}
          {chats.map((chat) => (
            <div key={chat.id}><ChatInfo name={chat.name}/></div>
          ))}
        </div>
     </div>
      <div className='messages'>ChatsMainPage</div>
      {/* <button onClick={handleLogout}>Logout</button> */}
    </div>
  );
}
