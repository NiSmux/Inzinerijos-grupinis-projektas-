// Multiple To-do-list boards

import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './BoardsList.css';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

function BoardsList() {
  const [boards, setBoards] = useState([]);
  const [newBoardTitle, setNewBoardTitle] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    fetchBoards();
  }, []);

  const fetchBoards = async () => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      navigate('/');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/boards`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Failed to fetch boards');
      }

      const data = await response.json();
      setBoards(data);
    } catch (err) {
      console.error('Error fetching boards:', err);
    }
  };

  const handleCreateBoard = async (e) => {
    e.preventDefault();
    
    if (!newBoardTitle.trim()) {
      return;
    }

    const token = localStorage.getItem('authToken');
    if (!token) {
      navigate('/');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/boards`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          title: newBoardTitle
        })
      });

      if (!response.ok) {
        throw new Error('Failed to create board');
      }

      const newBoard = await response.json();
      setBoards([...boards, newBoard]);
      setNewBoardTitle('');
    } catch (err) {
      console.error('Error creating board:', err);
    }
  };

  const handleDeleteBoard = async (boardId) => {
    const token = localStorage.getItem('authToken');
    if (!token) return;

    if (!confirm('Are you sure you want to delete this board? This will also delete all tasks in this board.')) {
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/boards/${boardId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Failed to delete board');
      }

      // Update the boards list
      setBoards(boards.filter(board => board.id !== boardId));
    } catch (err) {
      console.error('Error deleting board:', err);
    }
  };

  return (
    <div className="boards-list-container">
      <div className="boards-background">
        {Array.from({ length: 50 }).map((_, i) => (
          <span
            key={i}
            style={{
              left: `${Math.random() * 100}vw`,
              animationDelay: `${Math.random() * 10}s`,
            }}
          ></span>
        ))}
      </div>
      
      <div className="boards-content">
        <h1>My Task Boards</h1>
        
        <div className="create-board-form">
          <form onSubmit={handleCreateBoard}>
            <input
              type="text"
              placeholder="New board name..."
              value={newBoardTitle}
              onChange={(e) => setNewBoardTitle(e.target.value)}
            />
            <button type="submit" className="create-board-btn">Create Board</button>
          </form>
        </div>
        
        <div className="boards-grid">
          {boards.length === 0 ? (
            <div className="no-boards-message">
              <p>You don't have any task boards yet. Create one to get started!</p>
            </div>
          ) : (
            boards.map(board => (
              <div key={board.id} className="board-card">
                <Link to={`/board/${board.id}`} className="board-title">
                  {board.title}
                </Link>
                <div className="board-meta">
                  <span className="board-date">Created: {new Date(board.createdAt).toLocaleDateString()}</span>
                  <button 
                    className="delete-board-btn"
                    onClick={() => handleDeleteBoard(board.id)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}

export default BoardsList;
