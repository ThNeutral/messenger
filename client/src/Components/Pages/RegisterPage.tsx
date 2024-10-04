import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
export const RegisterPage = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [errors, setErrors] = useState<string[]>([]);
  const handleUsernameChange = (e: React.ChangeEvent<HTMLInputElement>) => setUsername(e.target.value);
  const handlePasswordChange = (e : React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value);
  const handleEmailChange = (e : React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
        const response = await fetch('http://localhost:3000/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({"username": username, "email": email, "password": password }),
        });

        if (response.ok) {
            // navigate('/');
            setUsername('')
            setPassword('')
            setEmail('')
            setErrors([])


        } else {
            const errorData = await response.json();
            setErrors([errorData.message]);
        }
    } catch (error) {
        setErrors(['An unexpected error occurred']);
    }
};
  console.log(errors)
  

  return (
    <div className='sign-form'>
      <div className={errors.length !== 0? 'sign-form-container error' : 'sign-form-container'}><p className='p-h1'>Registration</p>
      {/* <div> */}
        <form onSubmit={handleSubmit}>
          <div className='input-container'> 
        <input
          type="text"
          placeholder="Email"
          value={email}
          id='email'
          onChange={handleEmailChange}
        />
        <label htmlFor='email'>Input your email</label></div>
       <div className='input-container'>
        <input
          type="text"
          placeholder="Username"
          id='username'
          value={username}
          onChange={handleUsernameChange}
        />
        <label htmlFor='username'>Input your username</label></div>
        
        <div className='input-container'>
        <input
          type="password"
          id='password'
          placeholder="Password"
          value={password}
          onChange={handlePasswordChange}
        />
        <label htmlFor='password'>Input your password</label></div>
        
         {errors.length > 0 && (
                <ul className='error'>
                    {errors.map((err, index) => (
                        <li
                            key={index}
                            className={`${errors.length === 1 ? 'li-error': ''}`}
                        >
                            {err}
                        </li>
                    ))}
                </ul>
            )}
         <p className='question'>Already have an account? <a href='/username'> Sign in</a></p>
        <button className={errors.length === 0 ? '' : 'button-error'}  type="submit">Sign up</button>
      </form></div>
      
    </div>
  );
};
