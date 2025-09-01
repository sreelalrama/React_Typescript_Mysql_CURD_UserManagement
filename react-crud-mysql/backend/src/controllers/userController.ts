import { Request, Response } from 'express';
import { connection } from '../config/database';
import { CreateUserRequest, UpdateUserRequest, User } from '../models/User';

export const getAllUsers = async (req: Request, res: Response) => {
  try {
    const conn = await connection;
    const [rows] = await conn.execute('SELECT * FROM users ORDER BY id DESC');
    res.json(rows);
  } catch (error) {
    console.error('Error fetching users:', error);
    res.status(500).json({ error: 'Failed to fetch users' });
  }
};

export const getUserById = async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const conn = await connection;
    const [rows] = await conn.execute('SELECT * FROM users WHERE id = ?', [id]);
    
    const users = rows as User[];
    if (users.length === 0) {
      return res.status(404).json({ error: 'User not found' });
    }
    
    res.json(users[0]);
  } catch (error) {
    console.error('Error fetching user:', error);
    res.status(500).json({ error: 'Failed to fetch user' });
  }
};

export const createUser = async (req: Request, res: Response) => {
  try {
    const { name, email, age }: CreateUserRequest = req.body;
    
    if (!name || !email || !age) {
      return res.status(400).json({ error: 'Name, email, and age are required' });
    }
    
    if (age <= 0 || age > 150) {
      return res.status(400).json({ error: 'Age must be between 1 and 150' });
    }
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return res.status(400).json({ error: 'Invalid email format' });
    }
    
    const conn = await connection;
    const [result] = await conn.execute(
      'INSERT INTO users (name, email, age) VALUES (?, ?, ?)',
      [name, email, age]
    );
    
    const insertResult = result as any;
    const newUserId = insertResult.insertId;
    
    const [rows] = await conn.execute('SELECT * FROM users WHERE id = ?', [newUserId]);
    const users = rows as User[];
    
    res.status(201).json(users[0]);
  } catch (error: any) {
    console.error('Error creating user:', error);
    if (error.code === 'ER_DUP_ENTRY') {
      res.status(409).json({ error: 'Email already exists' });
    } else {
      res.status(500).json({ error: 'Failed to create user' });
    }
  }
};

export const updateUser = async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const { name, email, age }: UpdateUserRequest = req.body;
    
    const conn = await connection;
    const [existingUser] = await conn.execute('SELECT * FROM users WHERE id = ?', [id]);
    const users = existingUser as User[];
    
    if (users.length === 0) {
      return res.status(404).json({ error: 'User not found' });
    }
    
    if (age !== undefined && (age <= 0 || age > 150)) {
      return res.status(400).json({ error: 'Age must be between 1 and 150' });
    }
    
    if (email !== undefined) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        return res.status(400).json({ error: 'Invalid email format' });
      }
    }
    
    const updates: string[] = [];
    const values: any[] = [];
    
    if (name !== undefined) {
      updates.push('name = ?');
      values.push(name);
    }
    if (email !== undefined) {
      updates.push('email = ?');
      values.push(email);
    }
    if (age !== undefined) {
      updates.push('age = ?');
      values.push(age);
    }
    
    if (updates.length === 0) {
      return res.status(400).json({ error: 'No fields to update' });
    }
    
    values.push(id);
    
    await conn.execute(
      `UPDATE users SET ${updates.join(', ')} WHERE id = ?`,
      values
    );
    
    const [rows] = await conn.execute('SELECT * FROM users WHERE id = ?', [id]);
    const updatedUsers = rows as User[];
    
    res.json(updatedUsers[0]);
  } catch (error: any) {
    console.error('Error updating user:', error);
    if (error.code === 'ER_DUP_ENTRY') {
      res.status(409).json({ error: 'Email already exists' });
    } else {
      res.status(500).json({ error: 'Failed to update user' });
    }
  }
};

export const deleteUser = async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const conn = await connection;
    
    const [existingUser] = await conn.execute('SELECT * FROM users WHERE id = ?', [id]);
    const users = existingUser as User[];
    
    if (users.length === 0) {
      return res.status(404).json({ error: 'User not found' });
    }
    
    await conn.execute('DELETE FROM users WHERE id = ?', [id]);
    res.json({ message: 'User deleted successfully' });
  } catch (error) {
    console.error('Error deleting user:', error);
    res.status(500).json({ error: 'Failed to delete user' });
  }
};