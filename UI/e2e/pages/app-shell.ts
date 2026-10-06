import type { Locator, Page } from '@playwright/test';

/** The authenticated ERP chrome: header, user menu and sidebar navigation. */
export class AppShell {
  readonly nav: Locator;
  readonly userMenu: Locator;

  constructor(private readonly page: Page) {
    this.nav = page.getByRole('navigation', { name: 'Main' });
    this.userMenu = page.getByRole('button', { name: 'User menu' });
  }

  navLink(name: string): Locator {
    return this.nav.getByRole('link', { name, exact: true });
  }

  async goTo(name: string): Promise<void> {
    await this.navLink(name).click();
  }

  async logout(): Promise<void> {
    await this.userMenu.click();
    await this.page.getByRole('menuitem', { name: 'Logout' }).click();
  }
}
