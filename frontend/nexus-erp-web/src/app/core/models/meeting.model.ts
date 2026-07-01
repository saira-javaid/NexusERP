export enum MeetingStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum MeetingAttendeeRole {
  Organizer = 0,
  Required = 1,
  Optional = 2,
}

export interface Meeting {
  id: string;
  title: string;
  description?: string;
  location?: string;
  startAt: string;
  endAt: string;
  status: MeetingStatus;
  organizerId: string;
  organizerName: string;
  projectId?: string;
  projectName?: string;
  attendeeCount: number;
  createdAt: string;
}

export interface MeetingAttendee {
  userId: string;
  fullName: string;
  email: string;
  role: MeetingAttendeeRole;
}

export interface MeetingDetail extends Meeting {
  attendees: MeetingAttendee[];
}

export interface MeetingAttendeeInput {
  userId: string;
  role: MeetingAttendeeRole;
}

export interface SaveMeetingRequest {
  title: string;
  description?: string;
  location?: string;
  startAt: string;
  endAt: string;
  status: MeetingStatus;
  projectId?: string;
  attendees: MeetingAttendeeInput[];
}
