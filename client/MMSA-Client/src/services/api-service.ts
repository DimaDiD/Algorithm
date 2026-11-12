import axios, { AxiosError, InternalAxiosRequestConfig } from "axios";
import BASE_URL from "../config";
import { tokenStore } from "../auth/tokenStore";
import { refreshAccessToken } from "../auth/authApi";
import notification from "../notification/notificationMessage";

interface HttpResponse {
  status: number;
  headers: any;
  data: any;
}

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean };

const apiClient = axios.create({ baseURL: BASE_URL });

apiClient.interceptors.request.use((config) => {
  const token = tokenStore.get();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetriableConfig | undefined;
    const status = error.response?.status;

    if (status === 401 && original && !original._retry) {
      original._retry = true;
      const newToken = await refreshAccessToken();
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(original);
      }
      notification("warning", "Сесія завершилась. Увійдіть знову.");
    } else if (status === 403) {
      notification("error", "Недостатньо прав для цієї дії.");
    } else if (status === 429) {
      notification("warning", "Забагато запитів. Спробуйте трохи пізніше.");
    }

    return Promise.reject(error);
  }
);

const get = async (url: string, data?: any, paramsSerializer?: any): Promise<HttpResponse> => {
  const response = await apiClient.get(url, {
    params: data,
    paramsSerializer: paramsSerializer
  });
  return response;
};

const post = async (url: string, data?: any) => {
  const response = await apiClient.post(url, data, {
    headers: {
      "Accept": "application/json",
      "Content-Type": 'application/json',
    },
  });
  return response;
};

const put = async (url: string, data?: any): Promise<HttpResponse> => {
  const response = await apiClient.put(url, data, {
    headers: {
      "Accept": "application/json",
      "Content-Type": "application/json",
    },
  });
  return response;
};

const remove = async (url: string, data?: any, options: any = { }): Promise<HttpResponse> => {
  const response = await apiClient.delete(url, {
    ...options,
    params: data,
  });
  return response;
};

const api = { get, post, put, remove };

export default api;
