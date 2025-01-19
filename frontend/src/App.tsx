import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Index from './pages/home/Index'
import Layout from './pages/shared/Layout'
import NotFound from './pages/shared/NotFound'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Index />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
