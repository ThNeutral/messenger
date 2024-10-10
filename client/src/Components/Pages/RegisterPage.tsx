import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export const RegisterPage = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [errors, setErrors] = useState<string[]>([]);
  const navigate = useNavigate();

  const handleUsernameChange = (e: React.ChangeEvent<HTMLInputElement>) => setUsername(e.target.value);
  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value);
  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value);

  const formSubmitValidation = (email: string, username: string, password: string): string[] => {
    const newErrors: string[] = [];

    if (email.length === 0 || username.length === 0 || password.length === 0) {
      newErrors.push('Fill in all fields');
    }
    if (username.length < 5) {
      newErrors.push('Username must be more than 5 characters long');
    }
    if (password.length < 8) {
      newErrors.push('Password must be more than 8 characters long');
    }

    return newErrors;
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = formSubmitValidation(email, username, password);
    if (validationErrors.length > 0) {
      setErrors(validationErrors);
      return; 
    }

    try {
      const response = await fetch('http://localhost:3000/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ username, email, password }),
      });

      if (response.ok) {
        try {
          const resp = await fetch('http://localhost:3000/login-username', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, password }),
          });

          if(resp.ok) {
            navigate('/chats')
          }
          setUsername('');
          setPassword('');
          setEmail('');
          setErrors([]);
          
        }
        
        catch(err) {
            console.log(err)
        }
      } else {
        setErrors(['Username or email are already taken']);
      }
    } catch (error) {
      setErrors([`${error}`]);
    }
  };

  return (
    <div className='sign-form'>
      <div className={errors.length !== 0 ? 'sign-form-container error' : 'sign-form-container'}>
        <p className='p-h1'>Registration</p>
        <form onSubmit={handleSubmit}>
          <div className='input-container'>
            <input
              type="text"
              placeholder="Email"
              value={email}
              id='email'
              onChange={handleEmailChange}
            />
            <label htmlFor='email'>Input your email</label>
          </div>
          <div className='input-container'>
            <input
              type="text"
              placeholder="Username"
              id='username'
              value={username}
              onChange={handleUsernameChange}
            />
            <label htmlFor='username'>Input your username</label>
          </div>

          <div className='input-container'>
            <input
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
          <p className='question'>Already have an account? <a href='/signin'>Sign in</a></p>
          <button type="submit" className={errors.length === 0 ? '' : 'button-error'}>
            Sign up
          </button>
        </form>
      </div>
    </div>
  );
};
