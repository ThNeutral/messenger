import './/assets/css/style.css'

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { RegisterPage } from './Components/Pages/RegisterPage';
import { SignInPage } from './Components/Pages/SignInPage';
import { ChatsMainPage } from './Components/Pages/ChatsMainPage';
function App() {
  return (
    <>
    <div>
      <main className='main-app'>
      <Router>
      <Routes>
        <Route path='/signup' element={<RegisterPage />}/>
        <Route path='/signin' element={<SignInPage />}/>
        <Route path='/chats' element={<ChatsMainPage />}/>
      </Routes>
      </Router>
      </main>
    </div>
    </>
  )
}

export default App
