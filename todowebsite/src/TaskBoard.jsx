import React, { useState, useEffect } from 'react';
import './TaskBoard.css'; // Optional external stylesheet

function TaskBoard() {
  // Initial tasks state – will be loaded from API
  const [tasks, setTasks] = useState([]);
  // State for tracking new task input
  const [newTaskTitle, setNewTaskTitle] = useState('');

  // useEffect – when the component mounts, call the API and set tasks from the response
  useEffect(() => {
    fetch('https://localhost:7066/api/tasks')
      .then(response => response.json())
      .then(data => {
        setTasks(data);
      })
      .catch(error => {
        console.error('Error fetching tasks from API:', error);
      });
  }, []);

  // Track the new task input and add a new task locally
  const handleAddTask = () => {
    if (!newTaskTitle.trim()) return;

    const newTask = {
      id: Date.now(), // Locally generated ID
      title: newTaskTitle,
      status: 'todo'
    };

    setTasks((prevTasks) => [...prevTasks, newTask]);
    setNewTaskTitle('');
  };

  // Remove a task locally
  const handleRemoveTask = (taskId) => {
    setTasks((prevTasks) => prevTasks.filter(task => task.id !== taskId));
  };

  // When drag starts, store the task id in the dataTransfer object
  const onDragStart = (e, taskId) => {
    e.dataTransfer.setData('text/plain', taskId);
  };

  // Allow dropping on an element (prevent default behavior)
  const onDragOver = (e) => {
    e.preventDefault();
  };

  // When dropping, retrieve the task id and update its status
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

      {/* Input for adding new tasks */}
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

      {/* Columns */}
      <div className="task-columns">
        {/* To Do Column */}
        <div
          className="task-column todo"
          onDragOver={onDragOver}
          onDrop={(e) => onDrop(e, 'todo')}
        >
          <h2>To Do</h2>
          {tasks
            .filter((task) => task.status === 'todo')
            .map((task) => (
              <div
                key={task.id}
                className="task"
                draggable
                onDragStart={(e) => onDragStart(e, task.id)}
              >
                {task.title}
                <button className="delete-button" onClick={() => handleRemoveTask(task.id)}>
                  ❌
                </button>
              </div>
            ))}
        </div>

        {/* In Progress Column */}
        <div
          className="task-column inprogress"
          onDragOver={onDragOver}
          onDrop={(e) => onDrop(e, 'inprogress')}
        >
          <h2>In Progress</h2>
          {tasks
            .filter((task) => task.status === 'inprogress')
            .map((task) => (
              <div
                key={task.id}
                className="task"
                draggable
                onDragStart={(e) => onDragStart(e, task.id)}
              >
                {task.title}
                <button className="delete-button" onClick={() => handleRemoveTask(task.id)}>
                  ❌
                </button>
              </div>
            ))}
        </div>

        {/* Done Column */}
        <div
          className="task-column done"
          onDragOver={onDragOver}
          onDrop={(e) => onDrop(e, 'done')}
        >
          <h2>Done</h2>
          {tasks
            .filter((task) => task.status === 'done')
            .map((task) => (
              <div
                key={task.id}
                className="task"
                draggable
                onDragStart={(e) => onDragStart(e, task.id)}
              >
                {task.title}
                <button className="delete-button" onClick={() => handleRemoveTask(task.id)}>
                  ❌
                </button>
              </div>
            ))}
        </div>
      </div>
    </div>
  );
}

export default TaskBoard;
