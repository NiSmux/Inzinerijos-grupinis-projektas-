import React, { useState, useEffect } from 'react';
import './TaskBoard.css';

function TaskBoard() {
  const [tasks, setTasks] = useState([]);
  const [newTaskTitle, setNewTaskTitle] = useState('');
  const [editingTaskId, setEditingTaskId] = useState(null);  // Track the task being edited
  const [editedTaskTitle, setEditedTaskTitle] = useState(''); // Store edited task title

  useEffect(() => {
    const fetchTasks = async () => {
      const token = localStorage.getItem('authToken');
      if (!token) {
        console.warn('No auth token found.');
        return;
      }
  
      try {
        const response = await fetch('http://localhost:5293/api/tasks', {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
  
        if (!response.ok) {
          throw new Error(`Failed to fetch tasks: ${response.status}`);
        }
  
        const data = await response.json();
        setTasks(data);
      } catch (err) {
        console.error('Error fetching tasks from API:', err.message);
      }
    };
  
    fetchTasks();
  }, []);

  const handleAddTask = async () => {
    if (!newTaskTitle.trim()) return;
  
    const token = localStorage.getItem('authToken');
    if (!token) {
      console.warn('No auth token found in localStorage');
      return;
    }
  
    try {
      const response = await fetch('http://localhost:5293/api/tasks', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          title: newTaskTitle,
          description: '',
          isCompleted: false,
          status: 'todo'
        })
      });
  
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to create task: ${response.status} - ${errorText}`);
      }
  
      const createdTask = await response.json();
      setTasks((prevTasks) => [...prevTasks, createdTask]);
      setNewTaskTitle('');
    } catch (err) {
      console.error('Error adding task:', err.message);
      alert('Error: ' + err.message);
    }
  };

  const handleRemoveTask = async (taskId) => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      console.warn('No auth token found in localStorage');
      return;
    }
  
    try {
      const response = await fetch(`http://localhost:5293/api/tasks/${taskId}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
  
      if (!response.ok) {
        throw new Error(`Failed to delete task: ${response.status}`);
      }
      setTasks(prevTasks => prevTasks.filter(task => task.id !== taskId));
    } catch (err) {
      console.error('Error deleting task:', err.message);
      alert('Error deleting task: ' + err.message);
    }
  };

  const handleEditTask = (taskId, title) => {
    setEditingTaskId(taskId);
    setEditedTaskTitle(title);
  };

  const handleSaveEdit = async (taskId) => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      console.warn('No auth token found in localStorage');
      return;
    }
  
    const currentTask = tasks.find(task => task.id === taskId);
    if (!currentTask) {
      console.error('Task not found for editing');
      return;
    }
  
    const updatedTask = {
      id: taskId,
      title: editedTaskTitle,
      description: currentTask.description || '',
      isCompleted: currentTask.isCompleted || false,
      status: currentTask.status,
      userId: currentTask.userId,
    };
  
    try {
      const response = await fetch(`http://localhost:5293/api/tasks/${taskId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(updatedTask),
      });
  
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to update task: ${response.status} - ${errorText}`);
      }

      setTasks(prevTasks =>
        prevTasks.map(task =>
          task.id === taskId ? { ...task, ...updatedTask } : task
        )
      );
  
      setEditingTaskId(null);
      setEditedTaskTitle('');
    } catch (err) {
      console.error('Error saving edited task:', err.message);
      alert('Error editing task: ' + err.message);
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
    const taskToUpdate = tasks.find(task => task.id === taskId);
  
    if (!taskToUpdate || taskToUpdate.status === newStatus) return;
  
    const updatedTask = {
      ...taskToUpdate,
      status: newStatus
    };
  
    try {
      const response = await fetch(`http://localhost:5293/api/tasks/${taskId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(updatedTask),
      });
  
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to update task status: ${response.status} - ${errorText}`);
      }
  
      setTasks((prevTasks) =>
        prevTasks.map((task) =>
          task.id === taskId ? { ...task, status: newStatus } : task
        )
      );
    } catch (err) {
      console.error('Error updating task status:', err.message);
      alert('Error updating status: ' + err.message);
    }
  };

  return (
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
                    <div>
                      <input
                        type="text"
                        value={editedTaskTitle}
                        onChange={(e) => setEditedTaskTitle(e.target.value)}
                      />
                      <button onClick={() => handleSaveEdit(task.id)}>Save</button>
                      <button onClick={handleCancelEdit}>Cancel</button>
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
  );
}

export default TaskBoard;
