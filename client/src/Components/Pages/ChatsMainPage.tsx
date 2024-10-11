import React, { useEffect, useState } from 'react';
// import { useNavigate } from 'react-router-dom'; 
import '../../assets/css/chatPage.css';
import { MenuIcon } from '../Icons/MenuIcon';
import { LoupeIcon } from '../Icons/LoupeIcon';
import { Chats } from '../../Interfaces/Chats';
import { Menu } from '../Pop up/Menu';
export const ChatsMainPage = () => {
  const [chats, setChats] = useState <Chats[]> ([]);
  const [chatName, setChatName] = useState<string>('');
  const [error, setError] = useState('');
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
const handleCreateChat = async () => {
  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch('https://localhost:3000/create-chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify({ chat_name: chatName }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      setError(errorData.errorMessage || 'Failed to create chat');
      return;
    }

    const newChat = await response.json();
    setChats([...chats, newChat]); 
    setChatName('');
  } catch (err) {
    setError('Failed to create chat');
  }
};


  return (
    <div className="chats-main-container">
      <div className='allChats'>
      <div className={`menu-open ${isMenuOpen ? 'show' : ''}`}>
          <Menu />
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
