import './/assets/css/style.css'

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { RegisterPage } from './Components/Pages/RegisterPage';
function App() {

  return (
    <>
    <div>
      <main className='main-app'>
      <Router>
      <Routes>
        {<Route path='/signup' element={<RegisterPage />}/>}
        {/* <Route path="/login" element={<Authorization />} />
        <Route path="/signup" element={<Registration />} />
        <Route path="/" element={<PrivateRoute/>} />
        <Route path='/profile' element={<Profile />}/> */}
      </Routes>
      </Router>
      </main>
    </div>
    </>
  )
}

export default App
