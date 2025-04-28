import React, { useState, useEffect } from 'react';
import './TaskBoard.css';
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/*
const { boardId } = useParams();

useEffect(() => {
  fetch(`/api/boards/${boardId}/tasks`)
    .then(res => res.json())
    .then(data => setTasks(data));
}, [boardId]);
*/

function TaskBoard() {
  const [tasks, setTasks] = useState([]);
  const [newTaskTitle, setNewTaskTitle] = useState('');
  const [editingTaskId, setEditingTaskId] = useState(null);
  const [editedTaskTitle, setEditedTaskTitle] = useState('');

  useEffect(() => {
    const fetchTasks = async () => {
      const token = localStorage.getItem('authToken');
      if (!token) return;

      try {
        const response = await fetch(`${API_BASE_URL}/api/tasks`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!response.ok) throw new Error();
        const data = await response.json();
        setTasks(data);
      } catch (err) {
        console.error('Error fetching tasks:', err.message);
      }
    };

    fetchTasks();
  }, []);

  const handleAddTask = async () => {
    if (!newTaskTitle.trim()) return;
    const token = localStorage.getItem('authToken');
    if (!token) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/tasks`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          title: newTaskTitle,
          description: '',
          isCompleted: false,
          status: 'todo',
        }),
      });
      if (!response.ok) throw new Error();
      const createdTask = await response.json();
      setTasks((prev) => [...prev, createdTask]);
      setNewTaskTitle('');
    } catch (err) {
      console.error('Error adding task:', err.message);
    }
  };

  const handleRemoveTask = async (taskId) => {
    const token = localStorage.getItem('authToken');
    if (!token) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/tasks/${taskId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!response.ok) throw new Error();
      setTasks((prev) => prev.filter((task) => task.id !== taskId));
    } catch (err) {
      console.error('Error deleting task:', err.message);
    }
  };

  const handleEditTask = (taskId, title) => {
    setEditingTaskId(taskId);
    setEditedTaskTitle(title);
  };

  const handleSaveEdit = async (taskId) => {
    const token = localStorage.getItem('authToken');
    if (!token) return;

    const updatedTask = tasks.find((task) => task.id === taskId);
    if (!updatedTask) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/tasks/${taskId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          ...updatedTask,
          title: editedTaskTitle,
        }),
      });
      if (!response.ok) throw new Error();
      setTasks((prev) =>
        prev.map((task) => (task.id === taskId ? { ...task, title: editedTaskTitle } : task))
      );
      setEditingTaskId(null);
      setEditedTaskTitle('');
    } catch (err) {
      console.error('Error saving edit:', err.message);
    }
  };

  const handleCancelEdit = () => {
    setEditingTaskId(null);
    setEditedTaskTitle('');
  };

  const onDragStart = (e, taskId) => {
    e.dataTransfer.setData('text/plain', taskId);
  };

  const onDragOver = (e) => {
    e.preventDefault();
  };

  const onDrop = async (e, newStatus) => {
    e.preventDefault();
    const taskId = parseInt(e.dataTransfer.getData('text/plain'), 10);
    const token = localStorage.getItem('authToken');
    const taskToUpdate = tasks.find((task) => task.id === taskId);
    if (!taskToUpdate || taskToUpdate.status === newStatus) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/tasks/${taskId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ ...taskToUpdate, status: newStatus }),
      });
      if (!response.ok) throw new Error();
      setTasks((prev) =>
        prev.map((task) => (task.id === taskId ? { ...task, status: newStatus } : task))
      );
    } catch (err) {
      console.error('Error updating task status:', err.message);
    }
  };

  return (
    <>
      {/* MOVING BACKGROUND */}
      <div className="taskboard-background">
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

      {/* TASK BOARD */}
      <div className="task-board">
        <h1>Task Management Board</h1>

        <div className="task-input">
          <input
            type="text"
            placeholder="Enter a new task..."
            value={newTaskTitle}
            onChange={(e) => setNewTaskTitle(e.target.value)}
          />
          <button className="rounded-button" onClick={handleAddTask}>
            Add Task
          </button>
        </div>

        <div className="task-columns">
          {['todo', 'inprogress', 'done'].map((status) => (
            <div
              className={`task-column ${status}`}
              key={status}
              onDragOver={onDragOver}
              onDrop={(e) => onDrop(e, status)}
            >
              <h2>{status.charAt(0).toUpperCase() + status.slice(1)}</h2>
              {tasks
                .filter((task) => task.status === status)
                .map((task) => (
                  <div
                    key={task.id}
                    className="task"
                    draggable
                    onDragStart={(e) => onDragStart(e, task.id)}
                  >
                    {editingTaskId === task.id ? (
                      <div className="edit-mode">
                        <input
                          className="edit-input"
                          type="text"
                          value={editedTaskTitle}
                          onChange={(e) => setEditedTaskTitle(e.target.value)}
                        />
                        <div className="edit-buttons">
                          <button className="save-button" onClick={() => handleSaveEdit(task.id)}>💾 Save</button>
                          <button className="cancel-button" onClick={handleCancelEdit}>❌ Cancel</button>
                        </div>
                      </div>
                    ) : (
                      <>
                        {task.title}
                        <button
                          className="edit-button"
                          onClick={() => handleEditTask(task.id, task.title)}
                        >
                          ✏️
                        </button>
                        <button
                          className="delete-button"
                          onClick={() => handleRemoveTask(task.id)}
                        >
                          ❌
                        </button>
                      </>
                    )}
                  </div>
                ))}
            </div>
          ))}
        </div>
      </div>
    </>
  );
}

export default TaskBoard;
