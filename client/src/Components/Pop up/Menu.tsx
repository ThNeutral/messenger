import React, { useEffect, useState } from 'react'
import '../../assets/css/menu.css'
import { GroupIcon } from '../Icons/GroupIcon'
import { ChatIcon } from '../Icons/ChatIcon'
import { SettingsIcon } from '../Icons/Settings'
import { CreateChat } from './CreateChat'

type MenuProps = {
    isMenuOpenRef: boolean,
    setIsMenuOpen: (v: boolean) => void;
  }
export const Menu = ({isMenuOpenRef, setIsMenuOpen} : MenuProps) => {
    const [username, setUsername] = useState<string>()
    const [isCreateChatOpen, setIsCreateChatOpen] = useState(false);
    const toggleCreateChat = () => {
        setIsCreateChatOpen((prev) => !prev);
        console.log(isCreateChatOpen)
    }  
    useEffect(() => {
        if(!isMenuOpenRef) setIsCreateChatOpen(false);
    }, [isMenuOpenRef])
    useEffect(() => {
        const fetchUserData = async () => {
            const token = localStorage.getItem('authToken');
            const response = await fetch('http://localhost:3000/me', {
                method: 'GET',
                headers: {
                  'Content-Type': 'application/json',
                  'Authorization': `Bearer ${token}`
                }
            });
        
            if (response.ok) {
                const user = await response.json();
                setUsername(user.username);
            } else if (response.status === 404) {
                console.error('User not found: Redirecting to login');
                window.location.href = '/signin';
            } else if (response.status === 401) {
                console.error('Unauthorized: Redirecting to login');
                window.location.href = '/signin';
            }
        };

        fetchUserData();
    }, []);
  return (
    <><div className='menu-main-container'>
        <h1>{username}</h1>
        <div className='line-btn'>
            <GroupIcon color={'#EBEFEC'} hoveredColor = {"#EBEFEC"}/>
            <span>New group</span>
        </div>
        <div className='line-btn' onClick={toggleCreateChat}>
            <ChatIcon color={'#EBEFEC'} hoveredColor = {"#EBEFEC"}/>
            <span>New chat</span>
        </div>
        <div className='line-btn'>
            <SettingsIcon color={'#EBEFEC'} hoveredColor = {"#EBEFEC"}/>
            <span>Settings</span>
        </div>
    </div>
    <div className={`create-chat ${isCreateChatOpen ? 'show': ''}`}><CreateChat setIsMenuOpen={setIsMenuOpen} /></div>
    </>
    
    
  )
}
