import { Directive, inject, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { UserService } from '../../core/services/user-service';

@Directive({
  selector: '[appHasRole]',
})
export class HasRole implements OnInit {
  @Input() appHasRole: string[] = [];
  private accountService = inject(UserService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef);
  
  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }
}
