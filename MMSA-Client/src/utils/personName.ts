export type PersonName = {
  LastName?: string | null;
  FirstName?: string | null;
  MiddleName?: string | null;
};

export const PERSON_NAME_MAX_LENGTH = 50;

export const fullName = ({ LastName, FirstName, MiddleName }: PersonName): string | null =>
  [LastName, FirstName, MiddleName].filter((part) => part?.trim()).join(" ") || null;

export const initials = (name: string | null | undefined, fallback: string): string =>
  (name?.trim() || fallback).charAt(0).toUpperCase();
