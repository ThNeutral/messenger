import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export const SignInPage = () => {
    const [usernameOrEmail, setUsernameOrEmail] = useState('');
    const [password, setPassword] = useState('');
    const [errors, setErrors] = useState<string[]>([]);
    const navigate = useNavigate();
  
    const handleUsernameOrEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => setUsernameOrEmail(e.target.value);
    const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value);
  
    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
       
      try {
        if (usernameOrEmail.length === 0 || password.length === 0) {
            setErrors(['Fill in empty fields']);
        } else {
            const resp = await fetch('http://localhost:3000/login-username', {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username: usernameOrEmail, password }),
            });
          
            if (resp.ok) {
                const data = await resp.json();
                localStorage.setItem('authToken', data.token); 
                navigate("/chats");
                setUsernameOrEmail('');
                setPassword('');
                setErrors([]);
            } else {
                const resp = await fetch('http://localhost:3000/login-email', {
                    method: 'POST',
                    headers: {
                      'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ email: usernameOrEmail, password }),
                });
                if (resp.ok) {
                    const data = await resp.json();
                    localStorage.setItem('authToken', data.token); 
                    navigate("/chats");
                    setUsernameOrEmail('');
                    setPassword('');
                    setErrors([]);
                } else {
                    setErrors(["User not found"]);
                }
            }
        }
      } catch (error) {
        setErrors([`${error}`]);
      }
    };
  
    return (
      <div className='sign-form'>
        <div className={errors.length !== 0 ? 'sign-form-container error' : 'sign-form-container'}>
          <p className='p-h1'>Sign in</p>
          <form onSubmit={handleSubmit}>
            <div className='input-container'>
              <input className='sign-in-input'
                type="text"
                placeholder="Login or email"
                id='usernameOrEmail'
                value={usernameOrEmail}
                onChange={handleUsernameOrEmailChange}
              />
              <label htmlFor='username'>Input your login or email</label>
            </div>
  
            <div className='input-container'>
              <input className='sign-in-input'
                type="password"
                id='password'
                placeholder="Password"
                value={password}
                onChange={handlePasswordChange}
              />
              <label htmlFor='password'>Input your password</label>
            </div>
  
            {errors.length > 0 && (
              <ul className='error'>
                {errors.map((err, index) => (
                  <li key={index} className={`${errors.length === 1 ? 'li-error' : ''}`}>
                    {err}
                  </li>
                ))}
              </ul>
            )}
            <p className='question'>Doesn't have an account? <a href='/signup'>Sign up</a></p>
            <button type="submit" className={errors.length === 0 ? 'sign-in-btn-submit' : 'button-error'}>
              Sign in
            </button>
          </form>
        </div>
      </div>
    );
};
