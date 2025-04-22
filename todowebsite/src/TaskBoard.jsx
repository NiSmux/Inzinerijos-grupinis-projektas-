import React, { useState, useEffect } from 'react';
import './TaskBoard.css';

function TaskBoard() {
  const [tasks, setTasks] = useState([]);
  const [newTaskTitle, setNewTaskTitle] = useState('');
  const [editingTaskId, setEditingTaskId] = useState(null);  // Track the task being edited
  const [editedTaskTitle, setEditedTaskTitle] = useState(''); // Store edited task title

  useEffect(() => {
    fetch('http://localhost:5293/api/tasks')
      .then(response => response.json())
      .then(data => {
        setTasks(data);
      })
      .catch(error => {
        console.error('Error fetching tasks from API:', error);
      });
  }, []);

  const handleAddTask = () => {
    if (!newTaskTitle.trim()) return;

    const newTask = {
      id: Date.now(),
      title: newTaskTitle,
      status: 'todo',
    };

    setTasks((prevTasks) => [...prevTasks, newTask]);
    setNewTaskTitle('');
  };

  const handleRemoveTask = (taskId) => {
    setTasks((prevTasks) => prevTasks.filter(task => task.id !== taskId));
  };

  const handleEditTask = (taskId, title) => {
    setEditingTaskId(taskId);
    setEditedTaskTitle(title);
  };

  const handleSaveEdit = async (taskId) => {
    const updatedTask = {
      id: taskId,
      title: editedTaskTitle,
      status: tasks.find(task => task.id === taskId).status,
    };

    // Send updated task to the backend API
    await fetch(`http://localhost:5293/api/tasks/${taskId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updatedTask),
    });

    // Update the task locally
    setTasks(tasks.map(task =>
      task.id === taskId ? { ...task, title: editedTaskTitle } : task
    ));

    setEditingTaskId(null); // Exit editing mode
    setEditedTaskTitle('');
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

  const onDrop = (e, newStatus) => {
    e.preventDefault();
    const taskId = e.dataTransfer.getData('text/plain');

    setTasks((prevTasks) =>
      prevTasks.map((task) =>
        task.id === parseInt(taskId, 10)
          ? { ...task, status: newStatus }
          : task
      )
    );
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
