import React, { useState } from 'react';
// import { useNavigate } from 'react-router-dom';
export const RegisterPage = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [email, setEmail] = useState('');
  const [errors, setErrors] = useState<string[]>([]);
  const handleUsernameChange = (e: React.ChangeEvent<HTMLInputElement>) => setUsername(e.target.value);
  const handlePasswordChange = (e : React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value);
  const handleEmailChange = (e : React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value);
//   const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
        const response = await fetch('http://localhost:3000/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({username, email, password }),
        });

        if (response.ok) {
            // navigate('/');
            alert('successful')
        } else {
            const errorData = await response.json();
            setErrors(errorData.message);
        }
    } catch (error) {
        console.log(error)
        setErrors(['An unexpected error occurred']);
    }
};
  console.log(errors)
  

  return (
    <div className='sign-form'>
      <div className={errors.length !== 0? 'sign-form-container error' : 'sign-form-container'}><p>Registration</p>
        <form onSubmit={handleSubmit}>
        <p>Input your email</p>
        <input
          type="text"
          placeholder="Email"
          value={email}
          onChange={handleEmailChange}
        />
        <p>Input your username</p>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={handleUsernameChange}
        />
        <p>Input your password</p>
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={handlePasswordChange}
        />
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
