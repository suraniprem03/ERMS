import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import api from '../utils/api';

interface User {
  id: number;
  email: string;
  name: string;
  role: string;
  isPasswordChangeRequired: boolean;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (token: string, userData: User) => void;
  logout: () => void;
  updateUser: (data: Partial<User>) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(() => {
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));

  useEffect(() => {
    const hydrateUser = async () => {
      if (token && !user) {
        try {
          // If we have a token but lost user data, fetch it
          const res = await api.get('/auth/me');
          if (res.data.success) {
            const userData = res.data.data;
            const mappedUser = { ...userData, role: userData.roleCode || userData.roleName };
            localStorage.setItem('user', JSON.stringify(mappedUser));
            setUser(mappedUser);
          }
        } catch (err) {
          // If token is invalid or expired, log them out
          logout();
        }
      }
    };
    hydrateUser();
  }, [token]);

  const login = (newToken: string, userData: User) => {
    localStorage.setItem('token', newToken);
    localStorage.setItem('user', JSON.stringify(userData));
    setToken(newToken);
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
  };

  const updateUser = (data: Partial<User>) => {
    if (user) {
      const updatedUser = { ...user, ...data };
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
    }
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
