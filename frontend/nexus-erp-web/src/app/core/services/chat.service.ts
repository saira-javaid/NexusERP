import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ChatHistoryItem {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatMessageRequest {
  message: string;
  history?: ChatHistoryItem[];
  selectedProjectId?: string;
  pendingTaskTitle?: string;
}

export interface ProjectChoice {
  id: string;
  name: string;
}

export interface ChatMessageResponse {
  reply: string;
  toolsUsed: string[];
  provider: string;
  projectChoices?: ProjectChoice[];
  pendingTaskTitle?: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/chat`;

  sendMessage(request: ChatMessageRequest): Observable<ChatMessageResponse> {
    return this.http.post<ChatMessageResponse>(`${this.baseUrl}/message`, request);
  }
}
