import React, { useState } from 'react';

type CreateChatProps = {
  setIsMenuOpen: (v: boolean) => void;
};

export const CreateChat = ({ setIsMenuOpen }: CreateChatProps) => {
  const [chatName, setChatName] = useState<string>('');
  const [error, setError] = useState(false); 

  const handleChatNameChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setChatName(e.target.value);

  const cancelBtn = () => setIsMenuOpen(false);

  const handleCreateChat = async () => {
    if (!chatName.trim()) {
      setError(true); 
      return;
    }

    try {
      const token = localStorage.getItem('authToken');
      const response = await fetch('http://localhost:3000/create-chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ chat_name: chatName }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        setError(true); // Показать ошибку при неудачном запросе
        return;
      }

      // Успешное создание чата
      setChatName('');
      setIsMenuOpen(false);
    } catch (err) {
      console.log(err);
      setError(true); // Ошибка при запросе
    }
  };

  return (
    <div className="create-chat-main-container">
      <div className="form-container">
        <div className={!error ? 'input-container' : 'input-container error'}>
          <input
            type="text"
            placeholder="Chat"
            value={chatName}
            id="chatName"
            onChange={handleChatNameChange}
          />
          <label htmlFor="chatName">Input name of chat</label>
        </div>
        {/* {error && <p className="error-message">Chat name cannot be empty</p>} */}
        <div className="buttons-cr-chat">
          <span className="cancel-btn" onClick={cancelBtn}>
            Cancel
          </span>
          <span onClick={handleCreateChat}>Create</span>
        </div>
      </div>
    </div>
  );
};
