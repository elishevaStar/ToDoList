import React, { useState } from 'react';
import service from './service';

function Login({ onLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleLogin(e) {
    e.preventDefault();
    try {
      await service.login(username, password);

      // ניקוי השדות והודעת השגיאה לאחר התחברות מוצלחת
      setUsername(""); 
      setPassword(""); 
      setError(""); 

      onLogin(); // עדכון המצב לאחר התחברות
    } catch (err) {
      // ניקוי שני השדות במקרה של שגיאה
      setUsername(""); 
      setPassword(""); 
      setError("Login failed. Please check your credentials.");
    }
  }

  return (
    <div>
      <h2>Login</h2>
      <form onSubmit={handleLogin}>
        <input
          type="text"
          placeholder="Username"
          value={username} // מקושר ל-state
          onChange={(e) => setUsername(e.target.value)} // מעדכן את ה-state
        />
        <input
          type="password"
          placeholder="Password"
          value={password} // מקושר ל-state
          onChange={(e) => setPassword(e.target.value)} // מעדכן את ה-state
        />
        <button type="submit">Login</button>
      </form>
      {error && <p style={{ color: "red" }}>{error}</p>}
    </div>
  );
}

export default Login;
