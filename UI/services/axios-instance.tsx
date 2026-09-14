import SessionStorage from "@/components/session-storage";
import axios from "axios";

//const bearerToken = SessionStorage.getToken();

const axiosInstance = (token:string) => axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5213",
  headers: {
    "Content-Type": "application/json",
    "Authorization": "Bearer " + token,
  },
  // You can add interceptors here for auth tokens if needed
});

export default axiosInstance;
