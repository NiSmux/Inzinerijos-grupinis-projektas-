import Header from './Header.jsx'
import Footer from './Footer.jsx'
import TaskBoard from "./TaskBoard";


function App() {
  
  return(
    <>
      <Header></Header>
      <div className="App">
      <TaskBoard />
    </div>
      <Footer></Footer>
    </>
  );
}

export default App
