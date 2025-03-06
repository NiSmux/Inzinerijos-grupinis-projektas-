import React, { useState } from 'react';
import './TaskBoard.css'; // Optional external stylesheet

function TaskBoard() {
  // Example tasks
  const [tasks, setTasks] = useState([
    { id: 1, title: 'Task 1', status: 'todo' },
    { id: 2, title: 'Task 2', status: 'inprogress' },
    { id: 3, title: 'Task 3', status: 'done' },
  ]);

  // Track the new task input
  const [newTaskTitle, setNewTaskTitle] = useState('');

  // Handle adding a new task
  const handleAddTask = () => {
    if (!newTaskTitle.trim()) return;

    const newTask = {
      id: Date.now(),
      title: newTaskTitle,
      status: 'todo'
    };

    setTasks((prevTasks) => [...prevTasks, newTask]);
    setNewTaskTitle('');
  };

  // When drag starts, store the task id in the dataTransfer object
  const onDragStart = (e, taskId) => {
    e.dataTransfer.setData('text/plain', taskId);
  };

  // Needed to allow dropping on an element (default behavior prevents drop)
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
        <button onClick={handleAddTask}>Add Task</button>
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
              </div>
            ))}
        </div>
      </div>
    </div>
  );
}

export default TaskBoard;
