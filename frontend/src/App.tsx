import { BrowserRouter, Route, Routes } from 'react-router-dom'
import Index from './pages/home/Index'
import Layout from './pages/shared/Layout'
import NotFound from './pages/shared/NotFound'
import GlobalAppContextProvider from '@dn/shared/GlobalAppContext'
import System from '@dn/pages/home/System'

function App() {
  return (
    <BrowserRouter>
      <GlobalAppContextProvider>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Index />} />
            <Route path="system" element={<System />} />
            <Route path="*" element={<NotFound />} />
          </Route>
        </Routes>
      </GlobalAppContextProvider>
    </BrowserRouter>
  )
}

export default App
