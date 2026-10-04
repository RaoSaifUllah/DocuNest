import { en, type TranslationKey } from './en';

// Locale dictionaries can be added without changing presentation components.
// Layout uses logical CSS properties so a locale may set document.dir to rtl.
export function t(key: TranslationKey): string {
  return en[key];
}
