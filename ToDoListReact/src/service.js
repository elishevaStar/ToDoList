import axios from 'axios';

axios.defaults.baseURL = "https://todolistserver-otsh.onrender.com"

// הגדרת כתובת ה-API כ-default
const apiClient = axios.create({
  baseURL: "https://todolistserver-otsh.onrender.com/", 
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
  login: async (username, passwordHash) => {
    try {
      // שליחת הבקשה לשרת
      const response = await axios.post("/login", { username, passwordHash });
  
      // הדפסת התגובה המלאה ללוג לדיבוג
      console.log("Login response:", response);
  
      // בדיקת קיום התגובה
      if (!response || !response.data) {
        throw new Error("No response or data received from the server.");
      }
  
      // קבלת הטוקן מהתגובה
      const token = response.data.token;
  
      // בדיקת תקינות הטוקן
      if (!token) {
        throw new Error("Token not found in response data.");
      }
  
      // שמירת הטוקן ב-localStorage
      localStorage.setItem("jwtToken", token);
      console.log("Token saved:", token);
  
      // החזרת הנתונים
      return response.data;
    } catch (err) {
      // טיפול בשגיאות ודיבוג
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
