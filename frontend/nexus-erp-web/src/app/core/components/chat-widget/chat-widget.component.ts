import { Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChatHistoryItem, ChatService, ProjectChoice } from '../../services/chat.service';
import { ChatMarkdownPipe } from '../../pipes/chat-markdown.pipe';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  toolsUsed?: string[];
  provider?: string;
  projectChoices?: ProjectChoice[];
  pendingTaskTitle?: string;
}

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    ChatMarkdownPipe,
  ],
  templateUrl: './chat-widget.component.html',
  styleUrl: './chat-widget.component.scss',
})
export class ChatWidgetComponent {
  private readonly chatService = inject(ChatService);

  readonly isOpen = signal(false);
  readonly loading = signal(false);
  readonly messages = signal<ChatMessage[]>([
    {
      role: 'assistant',
      content: 'Hi! I\'m the NexusERP assistant. Ask me about projects, tasks, meetings, or say "create task Review docs" — I\'ll let you pick the project.',
      provider: 'NexusERP Agent',
    },
  ]);

  inputControl = new FormControl('', { nonNullable: true, validators: [Validators.required] });

  toggle(): void {
    this.isOpen.update(v => !v);
  }

  send(): void {
    const text = this.inputControl.value.trim();
    if (!text || this.loading()) return;

    const history: ChatHistoryItem[] = this.messages()
      .filter(m => m.role === 'user' || m.role === 'assistant')
      .slice(-10)
      .map(m => ({ role: m.role, content: m.content }));

    this.messages.update(msgs => [...msgs, { role: 'user', content: text }]);
    this.inputControl.reset();
    this.loading.set(true);

    this.chatService.sendMessage({ message: text, history }).subscribe({
      next: res => {
        this.messages.update(msgs => [...msgs, {
          role: 'assistant',
          content: res.reply,
          toolsUsed: res.toolsUsed,
          provider: res.provider,
          projectChoices: res.projectChoices,
          pendingTaskTitle: res.pendingTaskTitle,
        }]);
        this.loading.set(false);
      },
      error: () => {
        this.messages.update(msgs => [...msgs, {
          role: 'assistant',
          content: 'Sorry, something went wrong. Please try again.',
        }]);
        this.loading.set(false);
      },
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  selectProject(project: ProjectChoice, pendingTaskTitle: string): void {
    if (this.loading()) return;

    this.messages.update(msgs => [...msgs, {
      role: 'user',
      content: `Use project: ${project.name}`,
    }]);
    this.loading.set(true);

    this.chatService.sendMessage({
      message: `Create task in ${project.name}`,
      selectedProjectId: project.id,
      pendingTaskTitle,
    }).subscribe({
      next: res => {
        this.messages.update(msgs => [...msgs, {
          role: 'assistant',
          content: res.reply,
          toolsUsed: res.toolsUsed,
          provider: res.provider,
        }]);
        this.loading.set(false);
      },
      error: () => {
        this.messages.update(msgs => [...msgs, {
          role: 'assistant',
          content: 'Sorry, something went wrong. Please try again.',
        }]);
        this.loading.set(false);
      },
    });
  }
}
