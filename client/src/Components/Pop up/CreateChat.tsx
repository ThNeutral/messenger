import React, { useState } from 'react'

type CreateChatProps = {
  setIsMenuOpen: (v: boolean) => void;
}

export const CreateChat = ( {setIsMenuOpen} : CreateChatProps) => {
    const [chatName, setChatName] = useState<string>('');
    const [error, setError] = useState('');
    const handleChatNameChange = (e: React.ChangeEvent<HTMLInputElement>) => setChatName(e.target.value);

    const handleCreateChat = async () => {
        try {
          const token = localStorage.getItem('authToken');
          const response = await fetch('http://localhost:3000/create-chat', {
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
          setChatName('');
          setIsMenuOpen(false);
        } catch (err) {
          setError('Failed to create chat');
        }
      };
  return (
    <div className='create-chat-main-container'>
      <div className='form-container'>
        <div className='input-container'>
            <input
              type="text"
              placeholder="Chat"
              value={chatName}
              id='chatName'
              onChange={handleChatNameChange}
            />
            <label htmlFor='chatName'>Input name of chat</label>
            </div>
            <div className='buttons-cr-chat'>
                <span className='cancel-btn'>Cancel</span>
                <span onClick={handleCreateChat}>Create</span>
          </div>
        </div>
        
    </div>
  )
}
