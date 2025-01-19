import axios from 'axios';

// הגדרת כתובת ה-API כ-default
const apiClient = axios.create({
  baseURL: process.env.REACT_APP_API_URL, 
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor להוספת ה-JWT לכותרות
apiClient.interceptors.request.use(
  (config) => {
    // קבלת הטוקן מ-localStorage
    const token = localStorage.getItem("jwtToken");

    // הוספת הטוקן לכותרת Authorization
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    // טיפול בשגיאה בהגדרת הבקשה
    return Promise.reject(error);
  }
);

// Interceptor לטיפול בשגיאות בתגובה
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    console.error("API Error:", {
      message: error.message,
      status: error.response?.status,
      data: error.response?.data,
    });

    return Promise.reject(error);
  }
);

// פונקציות API
export default {
  // התחברות
  login: async (Username, PasswordHash) => {
    try {
      const response = await apiClient.post("/login", { Username, PasswordHash });
      const token = response.data.token; // בדוק שמבנה התגובה מתאים
      if (token) {
        localStorage.setItem("jwtToken", token); // שמירת הטוקן ב-localStorage
        console.log("Token saved:", token);
      }
      return response.data;
    } catch (err) {
      console.error("Login failed:", err.response?.data || err.message);
      throw err; // העברת השגיאה הלאה לטיפול בקומפוננטה
    }
  },

  // קבלת כל המשימות
  getTasks: async () => {
    const result = await apiClient.get("/items");
    console.log("getTasks response:", result.data); // בדוק מה מוחזר מהשרת
    return result.data;
  },

  // הוספת משימה חדשה
  addTask: async (name) => {
    console.log("addTask", name);
    const result = await apiClient.post("/items", { name, isComplete: false });
    return result.data;
  },

  // עדכון מצב השלמת משימה
  setCompleted: async (id, isComplete, name) => {
    console.log("setCompleted", { id, isComplete, name });
    const result = await apiClient.put(`/items/${id}`, { id, isComplete, name });
    return result.data;
  },

  // מחיקת משימה
  deleteTask: async (id) => {
    console.log("deleteTask", id);
    await apiClient.delete(`/items/${id}`);
    console.log(`Task ${id} deleted successfully.`);
  },
};
